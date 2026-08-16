using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер просмотра и управления складами.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
public class WarehousesController(SimpleFlowContext context) : Controller
{
    /// <summary>
    /// Отображает список складов.
    /// </summary>
    /// <returns>Представление со списком складов.</returns>
    public async Task<IActionResult> Index() => View(await context.Warehouses.AsNoTracking().OrderBy(x => x.Code).ToListAsync());

    /// <summary>
    /// Отображает форму создания склада.
    /// </summary>
    /// <returns>Представление формы создания склада.</returns>
    public IActionResult Create() => View(new Warehouse());

    /// <summary>
    /// Создаёт новый склад.
    /// </summary>
    /// <param name="warehouse">Данные создаваемого склада.</param>
    /// <returns>Переход к списку или форма с ошибками валидации.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        if (await context.Warehouses.AnyAsync(x => x.Code == warehouse.Code))
            ModelState.AddModelError(nameof(Warehouse.Code), "A warehouse with this code already exists.");
        if (!ModelState.IsValid) return View(warehouse);
        context.Add(warehouse); await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает форму редактирования склада.
    /// </summary>
    /// <param name="id">Идентификатор склада.</param>
    /// <returns>Представление формы или результат 404.</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await context.Warehouses.FindAsync(id);
        return warehouse is null ? NotFound() : View(warehouse);
    }

    /// <summary>
    /// Сохраняет изменения склада.
    /// </summary>
    /// <param name="id">Идентификатор склада.</param>
    /// <param name="warehouse">Изменённые данные склада.</param>
    /// <returns>Переход к списку, форма с ошибками или результат 404.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Warehouse warehouse)
    {
        if (id != warehouse.Id) return NotFound();
        if (await context.Warehouses.AnyAsync(x => x.Id != id && x.Code == warehouse.Code))
            ModelState.AddModelError(nameof(Warehouse.Code), "A warehouse with this code already exists.");
        if (!ModelState.IsValid) return View(warehouse);
        var existing = await context.Warehouses.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Code = warehouse.Code;
        existing.Name = warehouse.Name;
        existing.Address = warehouse.Address;
        existing.IsActive = warehouse.IsActive;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает подтверждение удаления склада.
    /// </summary>
    /// <param name="id">Идентификатор склада.</param>
    /// <returns>Представление подтверждения или результат 404.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        var warehouse = await context.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return warehouse is null ? NotFound() : View(warehouse);
    }

    /// <summary>
    /// Удаляет склад после подтверждения.
    /// </summary>
    /// <param name="id">Идентификатор склада.</param>
    /// <returns>Переход к списку складов.</returns>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var warehouse = await context.Warehouses.FindAsync(id);
        if (warehouse is not null) { context.Remove(warehouse); await context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
