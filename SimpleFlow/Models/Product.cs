using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleFlow.Models;

/// <summary>
/// Товар или материал в каталоге.
/// </summary>
public class Product : EntityBase
{
    [Required, StringLength(50)]
    /// <summary>
    /// Уникальный складской код товара.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(150)]
    /// <summary>
    /// Наименование товара.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    /// <summary>
    /// Описание товара.
    /// </summary>
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999999999999.99)]
    /// <summary>
    /// Цена за единицу товара.
    /// </summary>
    public decimal UnitPrice { get; set; }

    [Required, StringLength(20)]
    /// <summary>
    /// Единица измерения товара.
    /// </summary>
    public string UnitOfMeasure { get; set; } = "pcs";

    /// <summary>
    /// Признак активности товара.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
