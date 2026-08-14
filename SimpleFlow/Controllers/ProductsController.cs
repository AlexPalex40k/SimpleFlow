using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;

namespace SimpleFlow.Controllers;

public class ProductsController(SimpleFlowContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View(new Product());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (await context.Products.AnyAsync(x => x.Sku == product.Sku))
            ModelState.AddModelError(nameof(Product.Sku), "A product with this SKU already exists.");
        if (!ModelState.IsValid) return View(product);
        context.Add(product); await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await context.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

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
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is not null) { context.Remove(product); await context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
