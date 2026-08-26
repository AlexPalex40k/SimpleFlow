namespace SimpleFlow.Services.Inventory;

/// <summary>
/// Сервис проведения складских документов.
/// </summary>
public interface IInventoryPostingService
{
    /// <summary>
    /// Результат проведения складского документа.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат складской операции.</returns>
    Task<DatabaseOperationResult> PostAsync(
        int documentId,
        CancellationToken cancellationToken = default);
}