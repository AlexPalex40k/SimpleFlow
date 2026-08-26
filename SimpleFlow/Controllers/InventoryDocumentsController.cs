using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;
using SimpleFlow.Services;
using SimpleFlow.Services.Inventory;
using SimpleFlow.ViewModels.Inventory;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер управления складскими документами.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="databaseOperation">Сервис сохранения данных.</param>
/// <param name="postingService">Сервис проведения документов.</param>
public class InventoryDocumentsController(
    SimpleFlowContext context,
    IDatabaseOperationService databaseOperation,
    IInventoryPostingService postingService) : Controller
{
    /// <summary>
    /// Список складских документов.
    /// </summary>
    /// <returns>Представление со списком документов.</returns>
    public async Task<IActionResult> Index()
    {
        var documents = await context.InventoryDocuments
            .AsNoTracking()
            .Include(document => document.SourceWarehouse)
            .Include(document => document.DestinationWarehouse)
            .OrderByDescending(document => document.DocumentDate)
            .ThenByDescending(document => document.Id)
            .ToListAsync();

        return View(documents);
    }

    /// <summary>
    /// Карточка складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <returns>Представление документа либо результат 404.</returns>
    public async Task<IActionResult> Details(int id)
    {
        var document = await context.InventoryDocuments
            .AsNoTracking()
            .Include(item => item.SourceWarehouse)
            .Include(item => item.DestinationWarehouse)
            .Include(item => item.Lines)
                .ThenInclude(line => line.Product)
            .FirstOrDefaultAsync(item => item.Id == id);

        return document is null
            ? NotFound()
            : View(document);
    }

    /// <summary>
    /// Форма создания складского документа.
    /// </summary>
    /// <returns>Представление формы создания.</returns>
    public async Task<IActionResult> Create()
    {
        await PopulateSelectListsAsync();

        return View(new InventoryDocumentEditViewModel
        {
            Lines =
            [
                new InventoryDocumentLineEditViewModel
                {
                    Quantity = 1
                }
            ]
        });
    }

    /// <summary>
    /// Результат создания складского документа.
    /// </summary>
    /// <param name="model">Данные нового документа.</param>
    /// <returns>Переход к списку либо форма с ошибками.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryDocumentEditViewModel model)
    {
        model.DocumentNumber = model.DocumentNumber;

        await ValidateDocumentAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        var document = new InventoryDocument
        {
            DocumentNumber = model.DocumentNumber.Trim(),
            OperationType = model.OperationType,
            DocumentDate = model.DocumentDate,
            SourceWarehouseId = model.SourceWarehouseId,
            DestinationWarehouseId = model.DestinationWarehouseId,
            Description = model.Description,
            Status = InventoryDocumentStatus.Draft,
            Lines = model.Lines
                .Select(line => new InventoryDocumentLine
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost
                })
                .ToList()
        };

        context.InventoryDocuments.Add(document);

        var result = await databaseOperation.SaveChangesAsync();

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage!);

            await PopulateSelectListsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Складской документ создан.";

        return RedirectToAction(
            nameof(Details),
            new { id = document.Id });
    }

    /// <summary>
    /// Форма изменения складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <returns>Представление формы либо результат 404.</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var document = await context.InventoryDocuments
            .AsNoTracking()
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (document is null)
            return NotFound();

        if (document.Status != InventoryDocumentStatus.Draft)
        {
            TempData["ErrorMessage"] = "Изменять можно только черновики.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        await PopulateSelectListsAsync();

        return View(MapToEditModel(document));
    }

    /// <summary>
    /// Результат изменения складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <param name="model">Изменённые данные документа.</param>
    /// <returns>Переход к карточке, форма либо результат 404.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        InventoryDocumentEditViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var document = await context.InventoryDocuments
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (document is null)
            return NotFound();

        if (document.Status != InventoryDocumentStatus.Draft)
        {
            TempData["ErrorMessage"] = "Изменять можно только черновики.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        model.DocumentNumber =
            model.DocumentNumber?.Trim() ?? string.Empty;

        await ValidateDocumentAsync(model, id);

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return View(model);
        }

        document.DocumentNumber = model.DocumentNumber;
        document.OperationType = model.OperationType;
        document.DocumentDate = model.DocumentDate;
        document.SourceWarehouseId = model.SourceWarehouseId;
        document.DestinationWarehouseId =
            model.DestinationWarehouseId;
        document.Description = model.Description;

        context.InventoryDocumentLines.RemoveRange(document.Lines);

        document.Lines = model.Lines
            .Select(line => new InventoryDocumentLine
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost
            })
            .ToList();

        var result = await databaseOperation.SaveChangesAsync();

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage!);

            await PopulateSelectListsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Изменения складского документа сохранены.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    /// <summary>
    /// Подтверждение удаления складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <returns>Представление подтверждения либо результат 404.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        var document = await context.InventoryDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (document is null)
            return NotFound();

        if (document.Status != InventoryDocumentStatus.Draft)
        {
            TempData["ErrorMessage"] = "Удалять можно только черновики.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        return View(document);
    }

    /// <summary>
    /// Результат удаления складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <returns>Переход к списку документов.</returns>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var document = await context.InventoryDocuments
            .FirstOrDefaultAsync(item => item.Id == id);

        if (document is null)
            return RedirectToAction(nameof(Index));

        if (document.Status != InventoryDocumentStatus.Draft)
        {
            TempData["ErrorMessage"] = "Удалять можно только черновики.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        context.InventoryDocuments.Remove(document);

        var result = await databaseOperation.SaveChangesAsync();

        TempData[
            result.Succeeded
                ? "SuccessMessage"
                : "ErrorMessage"] =
            result.Succeeded
                ? "Складской документ удалён."
                : result.ErrorMessage;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Результат проведения складского документа.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Переход к карточке документа.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Post(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await postingService.PostAsync(
            id,
            cancellationToken);

        TempData[
            result.Succeeded
                ? "SuccessMessage"
                : "ErrorMessage"] =
            result.Succeeded
                ? "Складской документ успешно проведён."
                : result.ErrorMessage;

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    private async Task ValidateDocumentAsync(
        InventoryDocumentEditViewModel model,
        int? currentId = null)
    {
        if (!Enum.IsDefined(model.OperationType))
        {
            ModelState.AddModelError(
                nameof(model.OperationType),
                "Выбран неизвестный тип складской операции.");

            return;
        }

        if (await context.InventoryDocuments.AnyAsync(document =>
                document.Id != currentId &&
                document.DocumentNumber == model.DocumentNumber))
        {
            ModelState.AddModelError(
                nameof(model.DocumentNumber),
                "Документ с таким номером уже существует.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.Lines),
                "Добавьте хотя бы одну строку.");
        }

        if (model.Lines
            .GroupBy(line => line.ProductId)
            .Any(group => group.Count() > 1))
        {
            ModelState.AddModelError(
                nameof(model.Lines),
                "Один товар нельзя добавлять в документ несколько раз.");
        }

        var productIds = model.Lines
            .Select(line => line.ProductId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var validProductIds = await context.Products
            .Where(product =>
                productIds.Contains(product.Id) &&
                product.IsActive)
            .Select(product => product.Id)
            .ToListAsync();

        foreach (var productId in productIds.Except(validProductIds))
        {
            var lineIndex = model.Lines.FindIndex(
                line => line.ProductId == productId);

            ModelState.AddModelError(
                $"Lines[{lineIndex}].ProductId",
                "Выбранный товар не существует или неактивен.");
        }

        var warehouseIds = new[]
            {
                model.SourceWarehouseId,
                model.DestinationWarehouseId
            }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var validWarehouseIds = await context.Warehouses
            .Where(warehouse =>
                warehouseIds.Contains(warehouse.Id) &&
                warehouse.IsActive)
            .Select(warehouse => warehouse.Id)
            .ToListAsync();

        if (model.SourceWarehouseId.HasValue &&
            !validWarehouseIds.Contains(model.SourceWarehouseId.Value))
        {
            ModelState.AddModelError(
                nameof(model.SourceWarehouseId),
                "Исходный склад не существует или неактивен.");
        }

        if (model.DestinationWarehouseId.HasValue &&
            !validWarehouseIds.Contains(model.DestinationWarehouseId.Value))
        {
            ModelState.AddModelError(
                nameof(model.DestinationWarehouseId),
                "Склад назначения не существует или неактивен.");
        }

        switch (model.OperationType)
        {
            case InventoryOperationType.Receipt:
                model.SourceWarehouseId = null;
                break;

            case InventoryOperationType.Issue:
            case InventoryOperationType.Reservation:
            case InventoryOperationType.ReservationRelease:
                if (model.SourceWarehouseId is null)
                {
                    ModelState.AddModelError(
                        nameof(model.SourceWarehouseId),
                        "Выберите исходный склад.");
                }

                model.DestinationWarehouseId = null;
                break;

            case InventoryOperationType.Transfer:
                if (model.SourceWarehouseId is null ||
                    model.DestinationWarehouseId is null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Выберите исходный склад и склад назначения.");
                }
                else if (model.SourceWarehouseId ==
                         model.DestinationWarehouseId)
                {
                    ModelState.AddModelError(
                        nameof(model.DestinationWarehouseId),
                        "Склады должны отличаться.");
                }

                break;

            case InventoryOperationType.Adjustment:
                ModelState.AddModelError(
                    nameof(model.OperationType),
                    "Корректировка пока не поддерживается.");
                break;
        }
    }

    private async Task PopulateSelectListsAsync()
    {
        var warehouses = await context.Warehouses
            .AsNoTracking()
            .Where(warehouse => warehouse.IsActive)
            .OrderBy(warehouse => warehouse.Code)
            .ToListAsync();

        var products = await context.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Sku)
            .ToListAsync();

        ViewBag.Warehouses =
            new SelectList(warehouses, "Id", "Name");

        ViewBag.Products =
            new SelectList(products, "Id", "Name");

        ViewBag.OperationTypes = Enum
            .GetValues<InventoryOperationType>()
            .Where(operation =>
                operation != InventoryOperationType.Adjustment)
            .Select(operation => new SelectListItem
            {
                Value = ((int)operation).ToString(),
                Text = operation.ToString()
            })
            .ToList();
    }

    private static InventoryDocumentEditViewModel MapToEditModel(
        InventoryDocument document)
    {
        return new InventoryDocumentEditViewModel
        {
            Id = document.Id,
            DocumentNumber = document.DocumentNumber,
            OperationType = document.OperationType,
            Status = document.Status,
            DocumentDate = document.DocumentDate,
            SourceWarehouseId = document.SourceWarehouseId,
            DestinationWarehouseId =
                document.DestinationWarehouseId,
            Description = document.Description,
            Lines = document.Lines
                .Select(line =>
                    new InventoryDocumentLineEditViewModel
                    {
                        Id = line.Id,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        UnitCost = line.UnitCost
                    })
                .ToList()
        };
    }
}