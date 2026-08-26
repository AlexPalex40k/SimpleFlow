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
                "The inventory document was not found.");
        }

        if (document.Status == InventoryDocumentStatus.Posted)
        {
            return new DatabaseOperationResult(
                false,
                "The inventory document has already been posted.");
        }

        if (document.Status == InventoryDocumentStatus.Cancelled)
        {
            return new DatabaseOperationResult(
                false,
                "A cancelled inventory document cannot be posted.");
        }

        if (document.Lines.Count == 0)
        {
            return new DatabaseOperationResult(
                false,
                "The inventory document does not contain any products.");
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
                "The inventory balance was changed by another user. Refresh the page and try again.");
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
                "The inventory document could not be posted.");
        }
    }

    private static DatabaseOperationResult ValidateDocument(
    InventoryDocument document)
    {
        if (document.Lines.Any(line => line.Quantity <= 0))
        {
            return new DatabaseOperationResult(
                false,
                "Product quantity must be greater than zero.");
        }

        switch (document.OperationType)
        {
            case InventoryOperationType.Receipt:
                if (document.DestinationWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "A destination warehouse is required for a receipt.");
                }

                if (document.Lines.Any(line => line.UnitCost < 0))
                {
                    return new DatabaseOperationResult(
                        false,
                        "Product unit cost cannot be negative.");
                }

                break;

            case InventoryOperationType.Issue:
            case InventoryOperationType.Reservation:
            case InventoryOperationType.ReservationRelease:
                if (document.SourceWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "A source warehouse is required for this operation.");
                }

                break;

            case InventoryOperationType.Transfer:
                if (document.SourceWarehouseId is null ||
                    document.DestinationWarehouseId is null)
                {
                    return new DatabaseOperationResult(
                        false,
                        "Both warehouses are required for a transfer.");
                }

                if (document.SourceWarehouseId ==
                    document.DestinationWarehouseId)
                {
                    return new DatabaseOperationResult(
                        false,
                        "The source and destination warehouses must be different.");
                }

                break;

            case InventoryOperationType.Adjustment:
                return new DatabaseOperationResult(
                    false,
                    "Inventory adjustment rules have not been configured yet.");

            default:
                return new DatabaseOperationResult(
                    false,
                    "The inventory operation type is unknown.");
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
                    "This inventory operation type is not supported yet."))
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
                $"No inventory balance exists for product ID {line.ProductId} in the warehouse.");
        }

        var quantityAvailable =
            balance.QuantityOnHand - balance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"There is not enough available stock for product ID {line.ProductId}. " +
                $"Available: {quantityAvailable:N4}.");
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
                $"No inventory balance exists for product ID {line.ProductId} in the warehouse.");
        }

        var quantityAvailable =
            balance.QuantityOnHand - balance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"There is not enough available stock to create the reservation. " +
                $"Available: {quantityAvailable:N4}.");
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
                $"The inventory balance for product ID {line.ProductId} was not found.");
        }

        if (balance.QuantityReserved < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"Cannot release {line.Quantity:N4} from the reservation. " +
                $"Reserved: {balance.QuantityReserved:N4}.");
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
                $"No inventory balance exists for product ID {line.ProductId} in the source warehouse.");
        }

        var quantityAvailable =
            sourceBalance.QuantityOnHand -
            sourceBalance.QuantityReserved;

        if (quantityAvailable < line.Quantity)
        {
            return new DatabaseOperationResult(
                false,
                $"There is not enough available stock for the transfer. " +
                $"Available: {quantityAvailable:N4}.");
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
