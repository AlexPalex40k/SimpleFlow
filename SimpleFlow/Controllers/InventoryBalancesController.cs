using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleFlow.Data;

namespace SimpleFlow.Controllers;

/// <summary>
/// Контроллер просмотра складских остатков.
/// </summary>
/// <param name="context">Контекст базы данных приложения.</param>
public class InventoryBalancesController(
    SimpleFlowContext context) : Controller
{
    /// <summary>
    /// Список складских остатков.
    /// </summary>
    /// <param name="warehouseId">Фильтр по складу.</param>
    /// <returns>Представление со складскими остатками.</returns>
    public async Task<IActionResult> Index(int? warehouseId)
    {
        var query = context.InventoryBalances
            .AsNoTracking()
            .Include(balance => balance.Product)
            .Include(balance => balance.Warehouse)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(
                balance => balance.WarehouseId == warehouseId);
        }

        ViewBag.Warehouses = await context.Warehouses
            .AsNoTracking()
            .Where(warehouse => warehouse.IsActive)
            .OrderBy(warehouse => warehouse.Code)
            .ToListAsync();

        ViewBag.WarehouseId = warehouseId;

        var balances = await query
            .OrderBy(balance => balance.Warehouse!.Code)
            .ThenBy(balance => balance.Product!.Sku)
            .ToListAsync();

        return View(balances);
    }
}