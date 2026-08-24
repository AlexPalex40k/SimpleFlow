using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;

namespace SimpleFlow.Services.Inventory;

/// <summary>
/// Сервис изменения остатков при проведении складских документов.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="logger">Сервис журналирования.</param>
public sealed class InventoryPostingService(
    SimpleFlowContext context,
    ILogger<InventoryPostingService> logger)
    : IInventoryPostingService
{
    /// <inheritdoc />
    public async Task<DatabaseOperationResult> PostAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await context.InventoryDocuments
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(
                item => item.Id == documentId,
                cancellationToken);

        if (document is null)
        {
            return new DatabaseOperationResult(
                false,
                "Складской документ не найден.");
        }

        if (document.Status == InventoryDocumentStatus.Posted)
        {
            return new DatabaseOperationResult(
                false,
                "Складской документ уже проведён.");
        }

        if (document.Status == InventoryDocumentStatus.Cancelled)
        {
            return new DatabaseOperationResult(
                false,
                "Отменённый складской документ нельзя провести.");
        }

        if (document.Lines.Count == 0)
        {
            return new DatabaseOperationResult(
                false,
                "Складской документ не содержит товаров.");
        }

        var validationResult = ValidateDocument(document);

        if (!validationResult.Succeeded)
            return validationResult;

        await using var transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var line in document.Lines)
            {
                var result = await PostLineAsync(
                    document,
                    line,
                    cancellationToken);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    context.ChangeTracker.Clear();
                    return result;
                }
            }

            document.Status = InventoryDocumentStatus.Posted;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new DatabaseOperationResult(true);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();

            logger.LogWarning(
                exception,
                "Inventory balance concurrency conflict for document {DocumentId}.",
                documentId);

            return new DatabaseOperationResult(
                false,
                "Остаток был изменён другим пользователем. Обновите страницу и повторите операцию.");
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();

            logger.LogError(
                exception,
                "Inventory document {DocumentId} posting failed.",
                documentId);

            return new DatabaseOperationResult(
                false,
                "Не удалось провести складской документ.");
        }
    }

    private static DatabaseOperationResult ValidateDocument(
    InventoryDocument document)
    {
        if (document.Lines.Any(line => line.Quantity <= 0))
        {
            return new DatabaseOperationResult(
                false,
                "Количество товара должно быть больше нуля.");
        }

        switch (document.OperationType)
        {
            case InventoryOperationType.Receipt:
                if (document.DestinationWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "Для поступления необходимо выбрать склад назначения.");
                }

                if (document.Lines.Any(line => line.UnitCost < 0))
                {
                    return new DatabaseOperationResult(
                        false,
                        "Стоимость товара не может быть отрицательной.");
                }

                break;

            case InventoryOperationType.Issue:
            case InventoryOperationType.Reservation:
            case InventoryOperationType.ReservationRelease:
                if (document.SourceWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "Для операции необходимо выбрать исходный склад.");
                }

                break;

            case InventoryOperationType.Transfer:
                if (document.SourceWarehouseId is null ||
                    document.DestinationWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "Для перемещения необходимо выбрать оба склада.");
                }

                if (document.SourceWarehouseId ==
                    document.DestinationWarehouseId)
                {
                    return new DatabaseOperationResult(
                        false,
                        "Исходный склад и склад назначения должны отличаться.");
                }

                break;

            case InventoryOperationType.Adjustment:
                return new DatabaseOperationResult(
                    false,
                    "Правила корректировки остатков пока не настроены.");

            default:
                return new DatabaseOperationResult(
                    false,
                    "Неизвестный тип складской операции.");
        }

        return new DatabaseOperationResult(true);
    }

    private Task<DatabaseOperationResult> PostLineAsync(
    InventoryDocument document,
    InventoryDocumentLine line,
    CancellationToken cancellationToken)
    {
        return document.OperationType switch
        {
            InventoryOperationType.Receipt =>
                PostReceiptAsync(document, line, cancellationToken),

            InventoryOperationType.Issue =>
                PostIssueAsync(document, line, cancellationToken),

            InventoryOperationType.Transfer =>
                PostTransferAsync(document, line, cancellationToken),

            InventoryOperationType.Reservation =>
                PostReservationAsync(document, line, cancellationToken),

            InventoryOperationType.ReservationRelease =>
                PostReservationReleaseAsync(
                    document,
                    line,
                    cancellationToken),

            _ => Task.FromResult(
                new DatabaseOperationResult(
                    false,
                    "Данный тип складской операции пока не поддерживается."))
        };
    }

    private async Task<InventoryBalance> GetOrCreateBalanceAsync(
    int productId,
    int warehouseId,
    CancellationToken cancellationToken)
    {
        var trackedBalance = context.InventoryBalances.Local
            .SingleOrDefault(item =>
                item.ProductId == productId &&
                item.WarehouseId == warehouseId);

        if (trackedBalance is not null)
            return trackedBalance;

        var balance = await context.InventoryBalances
            .SingleOrDefaultAsync(
                item =>
                    item.ProductId == productId &&
                    item.WarehouseId == warehouseId,
                cancellationToken);

        if (balance is not null)
            return balance;

        balance = new InventoryBalance
        {
            ProductId = productId,
            WarehouseId = warehouseId
        };

        context.InventoryBalances.Add(balance);

        return balance;
    }

    private Task<InventoryBalance?> FindBalanceAsync(
    int productId,
    int warehouseId,
    CancellationToken cancellationToken)
    {
        return context.InventoryBalances.SingleOrDefaultAsync(
            item =>
                item.ProductId == productId &&
                item.WarehouseId == warehouseId,
            cancellationToken);
    }

    private async Task<DatabaseOperationResult> PostReceiptAsync(
        InventoryDocument document,
        InventoryDocumentLine line,
        CancellationToken cancellationToken)
    {
        var warehouseId = document.DestinationWarehouseId!.Value;

        var balance = await GetOrCreateBalanceAsync(
            line.ProductId,
            warehouseId,
            cancellationToken);

        ApplyReceipt(balance, line.Quantity, line.UnitCost);

        return new DatabaseOperationResult(true);
    }

    private async Task<DatabaseOperationResult> PostIssueAsync(
        InventoryDocument document,
        InventoryDocumentLine line,
        CancellationToken cancellationToken)
    {
        var warehouseId = document.SourceWarehouseId!.Value;

        var balance = await FindBalanceAsync(
            line.ProductId,
            warehouseId,
            cancellationToken);

        if (balance is null)
        {
            return new DatabaseOperationResult(
                false,
                $"На складе отсутствует остаток товара с ID {line.ProductId}.");
        }

        var quantityAvailable =
            balance.QuantityOnHand - balance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"Недостаточно доступного товара с ID {line.ProductId}. " +
                $"Доступно: {quantityAvailable:N4}.");
        }

        balance.QuantityOnHand -= line.Quantity;

        if (balance.QuantityOnHand == 0)
            balance.AverageCost = 0;

        return new DatabaseOperationResult(true);
    }

    private async Task<DatabaseOperationResult> PostReservationAsync(
        InventoryDocument document,
        InventoryDocumentLine line,
        CancellationToken cancellationToken)
    {
        var warehouseId = document.SourceWarehouseId!.Value;

        var balance = await FindBalanceAsync(
            line.ProductId,
            warehouseId,
            cancellationToken);

        if (balance is null)
        {
            return new DatabaseOperationResult(
                false,
                $"На складе отсутствует остаток товара с ID {line.ProductId}.");
        }

        var quantityAvailable =
            balance.QuantityOnHand - balance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"Недостаточно доступного товара для резервирования. " +
                $"Доступно: {quantityAvailable:N4}.");
        }

        balance.QuantityReserved += line.Quantity;

        return new DatabaseOperationResult(true);
    }

    private async Task<DatabaseOperationResult>
        PostReservationReleaseAsync(
            InventoryDocument document,
            InventoryDocumentLine line,
            CancellationToken cancellationToken)
    {
        var warehouseId = document.SourceWarehouseId!.Value;

        var balance = await FindBalanceAsync(
            line.ProductId,
            warehouseId,
            cancellationToken);

        if (balance is null)
        {
            return new DatabaseOperationResult(
                false,
                $"Остаток товара с ID {line.ProductId} не найден.");
        }

        if (balance.QuantityReserved < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"Нельзя снять резерв {line.Quantity:N4}. " +
                $"Зарезервировано: {balance.QuantityReserved:N4}.");
        }

        balance.QuantityReserved -= line.Quantity;

        return new DatabaseOperationResult(true);
    }

    private async Task<DatabaseOperationResult> PostTransferAsync(
        InventoryDocument document,
        InventoryDocumentLine line,
        CancellationToken cancellationToken)
    {
        var sourceWarehouseId = document.SourceWarehouseId!.Value;
        var destinationWarehouseId =
            document.DestinationWarehouseId!.Value;

        var sourceBalance = await FindBalanceAsync(
            line.ProductId,
            sourceWarehouseId,
            cancellationToken);

        if (sourceBalance is null)
        {
            return new DatabaseOperationResult(
                false,
                $"На исходном складе отсутствует товар с ID {line.ProductId}.");
        }

        var quantityAvailable =
            sourceBalance.QuantityOnHand -
            sourceBalance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"Недостаточно доступного товара для перемещения. " +
                $"Доступно: {quantityAvailable:N4}.");
        }

        var transferCost = sourceBalance.AverageCost;

        sourceBalance.QuantityOnHand -= line.Quantity;

        if (sourceBalance.QuantityOnHand == 0)
            sourceBalance.AverageCost = 0;

        var destinationBalance = await GetOrCreateBalanceAsync(
            line.ProductId,
            destinationWarehouseId,
            cancellationToken);

        ApplyReceipt(
            destinationBalance,
            line.Quantity,
            transferCost);

        return new DatabaseOperationResult(true);
    }

    private static void ApplyReceipt(
        InventoryBalance balance,
        decimal receivedQuantity,
        decimal receivedUnitCost)
    {
        var currentInventoryValue =
            balance.QuantityOnHand * balance.AverageCost;

        var receivedInventoryValue =
            receivedQuantity * receivedUnitCost;

        var newQuantityOnHand =
            balance.QuantityOnHand + receivedQuantity;

        var newInventoryValue =
            currentInventoryValue + receivedInventoryValue;

        balance.QuantityOnHand = newQuantityOnHand;

        balance.AverageCost = newQuantityOnHand == 0
            ? 0
            : decimal.Round(
                newInventoryValue / newQuantityOnHand,
                4,
                MidpointRounding.AwayFromZero);
    }

}