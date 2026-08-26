using System.ComponentModel.DataAnnotations;
using SimpleFlow.Models;

namespace SimpleFlow.ViewModels.Inventory;

/// <summary>
/// Данные формы складского документа.
/// </summary>
public class InventoryDocumentEditViewModel
{
    /// <summary>
    /// Идентификатор документа.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер документа.
    /// </summary>
    [Required]
    [StringLength(30)]
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
    /// Дата документа.
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime DocumentDate { get; set; } = DateTime.UtcNow.Date;

    /// <summary>
    /// Идентификатор исходного склада.
    /// </summary>
    public int? SourceWarehouseId { get; set; }

    /// <summary>
    /// Идентификатор склада назначения.
    /// </summary>
    public int? DestinationWarehouseId { get; set; }

    /// <summary>
    /// Примечание к документу.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Строки документа.
    /// </summary>
    public List<InventoryDocumentLineEditViewModel> Lines { get; set; }
        = [];
}