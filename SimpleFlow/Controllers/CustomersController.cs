using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;
using SimpleFlow.Services;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер просмотра и управления клиентами.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="databaseOperation">Сервис безопасного сохранения данных.</param>
public class CustomersController(
    SimpleFlowContext context,
    IDatabaseOperationService databaseOperation) : Controller
{
    /// <summary>
    /// Отображает список клиентов.
    /// </summary>
    /// <returns>Представление со списком клиентов.</returns>
    public async Task<IActionResult> Index() => View(await context.Customers.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    /// <summary>
    /// Отображает форму создания клиента.
    /// </summary>
    /// <returns>Представление формы создания клиента.</returns>
    public IActionResult Create() => View(new Customer());

    /// <summary>
    /// Создаёт нового клиента.
    /// </summary>
    /// <param name="customer">Данные создаваемого клиента.</param>
    /// <returns>Переход к списку или форма с ошибками валидации.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!string.IsNullOrWhiteSpace(customer.TaxNumber) && await context.Customers.AnyAsync(x => x.TaxNumber == customer.TaxNumber))
            ModelState.AddModelError(nameof(Customer.TaxNumber), "A customer with this tax number already exists.");
        if (!ModelState.IsValid) return View(customer);
        context.Add(customer);
        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(customer);
        }

        TempData["SuccessMessage"] = "Клиент успешно создан.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает форму редактирования клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <returns>Представление формы или результат 404.</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    /// <summary>
    /// Сохраняет изменения клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <param name="customer">Изменённые данные клиента.</param>
    /// <returns>Переход к списку, форма с ошибками или результат 404.</returns>
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
        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(customer);
        }

        TempData["SuccessMessage"] = "Изменения клиента сохранены.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Отображает подтверждение удаления клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <returns>Представление подтверждения или результат 404.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await context.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return customer is null ? NotFound() : View(customer);
    }

    /// <summary>
    /// Удаляет клиента после подтверждения.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <returns>Переход к списку клиентов.</returns>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer is not null)
        {
            context.Remove(customer);
            var result = await databaseOperation.SaveChangesAsync();
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Клиент удалён."
                : result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }
}
