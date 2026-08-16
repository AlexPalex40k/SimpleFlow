namespace SimpleFlow.Services;

/// <summary>
/// Сервис безопасного сохранения изменений базы данных.
/// </summary>
public interface IDatabaseOperationService
{
    /// <summary>
    /// Сохраняет изменения и преобразует ожидаемые ошибки базы данных в понятный результат.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат сохранения данных.</returns>
    Task<DatabaseOperationResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
