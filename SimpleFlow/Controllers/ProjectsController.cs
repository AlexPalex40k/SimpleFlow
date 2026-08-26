using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;
using SimpleFlow.Models;
using SimpleFlow.Services;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер просмотра и управления строительными проектами.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
/// <param name="databaseOperation">Сервис безопасного сохранения данных.</param>
public class ProjectsController(
    SimpleFlowContext context,
    IDatabaseOperationService databaseOperation) : Controller
{
    /// <summary>
    /// Список проектов с информацией о клиентах.
    /// </summary>
    /// <returns>Представление со списком проектов.</returns>
    public async Task<IActionResult> Index()
    {
        var projects = await context.Projects
            .AsNoTracking()
            .Include(project => project.Customer)
            .OrderBy(project => project.ProjectNumber)
            .ToListAsync();

        return View(projects);
    }

    /// <summary>
    /// Форма создания проекта.
    /// </summary>
    /// <returns>Представление формы создания проекта.</returns>
    public async Task<IActionResult> Create()
    {
        await PopulateCustomersAsync();
        return View(new Project());
    }

    /// <summary>
    /// Результат создания проекта.
    /// </summary>
    /// <param name="project">Данные создаваемого проекта.</param>
    /// <returns>Переход к списку либо форма с ошибками.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        await ValidateProjectAsync(project);
        if (!ModelState.IsValid)
        {
            await PopulateCustomersAsync(project.CustomerId);
            return View(project);
        }

        context.Projects.Add(project);
        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            await PopulateCustomersAsync(project.CustomerId);
            return View(project);
        }

        TempData["SuccessMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Форма редактирования проекта.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <returns>Представление формы либо результат 404.</returns>
    public async Task<IActionResult> Edit(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project is null) return NotFound();

        await PopulateCustomersAsync(project.CustomerId);
        return View(project);
    }

    /// <summary>
    /// Результат изменения проекта.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <param name="project">Изменённые данные проекта.</param>
    /// <returns>Переход к списку, форма с ошибками либо результат 404.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id) return NotFound();

        await ValidateProjectAsync(project, id);
        if (!ModelState.IsValid)
        {
            await PopulateCustomersAsync(project.CustomerId);
            return View(project);
        }

        var existing = await context.Projects.FindAsync(id);
        if (existing is null) return NotFound();

        existing.ProjectNumber = project.ProjectNumber;
        existing.Name = project.Name;
        existing.CustomerId = project.CustomerId;
        existing.Status = project.Status;
        existing.StartDate = project.StartDate;
        existing.PlannedEndDate = project.PlannedEndDate;
        existing.ActualEndDate = project.ActualEndDate;
        existing.Address = project.Address;
        existing.ContractAmount = project.ContractAmount;
        existing.ProjectManager = project.ProjectManager;
        existing.Description = project.Description;

        var result = await databaseOperation.SaveChangesAsync();
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            await PopulateCustomersAsync(project.CustomerId);
            return View(project);
        }

        TempData["SuccessMessage"] = "Project changes saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Подтверждение удаления проекта.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <returns>Представление подтверждения либо результат 404.</returns>
    public async Task<IActionResult> Delete(int id)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(item => item.Customer)
            .FirstOrDefaultAsync(item => item.Id == id);

        return project is null ? NotFound() : View(project);
    }

    /// <summary>
    /// Результат подтверждённого удаления проекта.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <returns>Переход к списку проектов.</returns>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project is not null)
        {
            context.Projects.Remove(project);
            var result = await databaseOperation.SaveChangesAsync();
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Project deleted."
                : result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCustomersAsync(int? selectedCustomerId = null)
    {
        var customers = await context.Customers
            .AsNoTracking()
            .Where(customer => customer.IsActive || customer.Id == selectedCustomerId)
            .OrderBy(customer => customer.Name)
            .ToListAsync();

        ViewBag.CustomerId = new SelectList(customers, "Id", "Name", selectedCustomerId);
    }

    private async Task ValidateProjectAsync(Project project, int? currentId = null)
    {
        if (await context.Projects.AnyAsync(item =>
                item.Id != currentId && item.ProjectNumber == project.ProjectNumber))
            ModelState.AddModelError(nameof(Project.ProjectNumber), "A project with this number already exists.");

        if (!await context.Customers.AnyAsync(customer => customer.Id == project.CustomerId))
            ModelState.AddModelError(nameof(Project.CustomerId), "Select an existing customer.");

        if (project.PlannedEndDate < project.StartDate)
            ModelState.AddModelError(nameof(Project.PlannedEndDate), "The planned end date cannot be earlier than the start date.");

        if (project.ActualEndDate < project.StartDate)
            ModelState.AddModelError(nameof(Project.ActualEndDate), "The actual end date cannot be earlier than the start date.");
    }
}
