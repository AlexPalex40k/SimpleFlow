using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.ViewModels.Inventory;

/// <summary>
/// Данные строки формы складского документа.
/// </summary>
public class InventoryDocumentLineEditViewModel
{
    /// <summary>
    /// Идентификатор строки документа.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Выберите товар.")]
    public int ProductId { get; set; }

    /// <summary>
    /// Количество товара.
    /// </summary>
    [Range(
        typeof(decimal),
        "0.0001",
        "99999.9999",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Количество должно быть больше нуля.")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Стоимость единицы товара.
    /// </summary>
    [Range(
        typeof(decimal),
        "0",
        "99999.9999",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Стоимость не может быть отрицательной.")]
    public decimal UnitCost { get; set; }
}