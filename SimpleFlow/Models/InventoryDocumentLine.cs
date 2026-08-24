using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleFlow.Models;

/// <summary>
/// Строка складского документа.
/// </summary>
public class InventoryDocumentLine : EntityBase
{
    /// <summary>
    /// Идентификатор документа.
    /// </summary>
    public int InventoryDocumentId { get; set; }

    /// <summary>
    /// Складской документ.
    /// </summary>
    public InventoryDocument? InventoryDocument { get; set; }

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Товар.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Количество товара.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0.0001, 99999.9999)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Стоимость единицы товара.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, 99999.9999)]
    public decimal UnitCost { get; set; }
}
