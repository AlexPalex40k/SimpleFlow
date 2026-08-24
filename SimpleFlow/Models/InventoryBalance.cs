using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleFlow.Models;

/// <summary>
/// Остаток товара на складе.
/// </summary>
public class InventoryBalance : EntityBase
{
    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Товар.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Идентификатор склада.
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Склад.
    /// </summary>
    public Warehouse? Warehouse { get; set; }

    /// <summary>
    /// Фактическое количество товара на складе.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, 99999.9999)]
    public decimal QuantityOnHand { get; set; }

    /// <summary>
    /// Зарезервированное количество товара.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, 99999.9999)]
    public decimal QuantityReserved { get; set; }

    /// <summary>
    /// Средняя стоимость единицы товара.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0, 99999.9999)]
    public decimal AverageCost { get; set; }

    /// <summary>
    /// Доступное количество товара.
    /// </summary>
    [NotMapped]
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    /// <summary>
    /// Версия записи для контроля одновременных изменений.
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}