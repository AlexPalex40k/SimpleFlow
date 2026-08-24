using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

/// <summary>
/// Склад или место хранения.
/// </summary>
public class Warehouse : EntityBase
{
    [Required, StringLength(30)]
    /// <summary>
    /// Уникальный код склада.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    /// <summary>
    /// Наименование склада.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    /// <summary>
    /// Адрес склада.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Признак активности склада.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Остатки товаров на складе.
    /// </summary>
    public ICollection<InventoryBalance> InventoryBalances { get; set; }
        = new List<InventoryBalance>();
}