using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;

namespace SimpleFlow.Controllers;

public class WarehousesController(SimpleFlowContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.Warehouses.AsNoTracking().OrderBy(x => x.Code).ToListAsync());
    public IActionResult Create() => View(new Warehouse());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        if (await context.Warehouses.AnyAsync(x => x.Code == warehouse.Code))
            ModelState.AddModelError(nameof(Warehouse.Code), "A warehouse with this code already exists.");
        if (!ModelState.IsValid) return View(warehouse);
        context.Add(warehouse); await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await context.Warehouses.FindAsync(id);
        return warehouse is null ? NotFound() : View(warehouse);
    }

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

    public async Task<IActionResult> Delete(int id)
    {
        var warehouse = await context.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return warehouse is null ? NotFound() : View(warehouse);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var warehouse = await context.Warehouses.FindAsync(id);
        if (warehouse is not null) { context.Remove(warehouse); await context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
