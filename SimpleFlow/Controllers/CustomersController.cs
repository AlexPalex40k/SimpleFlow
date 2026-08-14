using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;

namespace SimpleFlow.Controllers;

public class CustomersController(SimpleFlowContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.Customers.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    public IActionResult Create() => View(new Customer());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!string.IsNullOrWhiteSpace(customer.TaxNumber) && await context.Customers.AnyAsync(x => x.TaxNumber == customer.TaxNumber))
            ModelState.AddModelError(nameof(Customer.TaxNumber), "A customer with this tax number already exists.");
        if (!ModelState.IsValid) return View(customer);
        context.Add(customer);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id) return NotFound();
        if (!string.IsNullOrWhiteSpace(customer.TaxNumber) && await context.Customers.AnyAsync(x => x.Id != id && x.TaxNumber == customer.TaxNumber))
            ModelState.AddModelError(nameof(Customer.TaxNumber), "A customer with this tax number already exists.");
        if (!ModelState.IsValid) return View(customer);
        var existing = await context.Customers.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = customer.Name;
        existing.TaxNumber = customer.TaxNumber;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Address = customer.Address;
        existing.IsActive = customer.IsActive;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var customer = await context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer is not null) { context.Remove(customer); await context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
