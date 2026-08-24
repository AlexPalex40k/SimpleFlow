using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

/// <summary>
/// Документ складской операции.
/// </summary>
public class InventoryDocument : EntityBase
{
    /// <summary>
    /// Уникальный номер документа.
    /// </summary>
    [Required, StringLength(30)]
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Тип складской операции.
    /// </summary>
    public InventoryOperationType OperationType { get; set; }

    /// <summary>
    /// Статус документа.
    /// </summary>
    public InventoryDocumentStatus Status { get; set; }
        = InventoryDocumentStatus.Draft;

    /// <summary>
    /// Дата складской операции.
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime DocumentDate { get; set; } = DateTime.UtcNow.Date;

    /// <summary>
    /// Идентификатор исходного склада.
    /// </summary>
    public int? SourceWarehouseId { get; set; }

    /// <summary>
    /// Исходный склад.
    /// </summary>
    public Warehouse? SourceWarehouse { get; set; }

    /// <summary>
    /// Идентификатор целевого склада.
    /// </summary>
    public int? DestinationWarehouseId { get; set; }

    /// <summary>
    /// Целевой склад.
    /// </summary>
    public Warehouse? DestinationWarehouse { get; set; }

    /// <summary>
    /// Примечание к документу.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Строки складского документа.
    /// </summary>
    public ICollection<InventoryDocumentLine> Lines { get; set; }
        = new List<InventoryDocumentLine>();
}