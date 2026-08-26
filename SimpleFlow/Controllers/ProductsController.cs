using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;
using SimpleFlow.Services;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер просмотра и управления товарами.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="databaseOperation">Сервис безопасного сохранения данных.</param>
public class ProductsController(
    SimpleFlowContext context,
    IDatabaseOperationService databaseOperation) : Controller
{
    /// <summary>
    /// Отображает список товаров.
    /// </summary>
    /// <returns>Представление со списком товаров.</returns>
    public async Task<IActionResult> Index() => View(await context.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    /// <summary>
    /// Отображает форму создания товара.
    /// </summary>
    /// <returns>Представление формы создания товара.</returns>
    public IActionResult Create() => View(new Product());

    /// <summary>
    /// Создаёт новый товар.
    /// </summary>
    /// <param name="product">Данные создаваемого товара.</param>
    /// <returns>Переход к списку или форма с ошибками валидации.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (await context.Products.AnyAsync(x => x.Sku == product.Sku))
            ModelState.AddModelError(nameof(Product.Sku), "A product with this SKU already exists.");
        if (!ModelState.IsValid) return View(product);
        context.Add(product);
        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(product);
        }

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает форму редактирования товара.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <returns>Представление формы или результат 404.</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var product = await context.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    /// <summary>
    /// Сохраняет изменения товара.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="product">Изменённые данные товара.</param>
    /// <returns>Переход к списку, форма с ошибками или результат 404.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return NotFound();
        if (await context.Products.AnyAsync(x => x.Id != id && x.Sku == product.Sku))
            ModelState.AddModelError(nameof(Product.Sku), "A product with this SKU already exists.");
        if (!ModelState.IsValid) return View(product);
        var existing = await context.Products.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Sku = product.Sku;
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.UnitPrice = product.UnitPrice;
        existing.UnitOfMeasure = product.UnitOfMeasure;
        existing.IsActive = product.IsActive;
        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(product);
        }

        TempData["SuccessMessage"] = "Product changes saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает подтверждение удаления товара.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <returns>Представление подтверждения или результат 404.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return product is null ? NotFound() : View(product);
    }

    /// <summary>
    /// Удаляет товар после подтверждения.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <returns>Переход к списку товаров.</returns>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is not null)
        {
            context.Remove(product);
            var result = await databaseOperation.SaveChangesAsync();
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Product deleted."
                : result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }
}
