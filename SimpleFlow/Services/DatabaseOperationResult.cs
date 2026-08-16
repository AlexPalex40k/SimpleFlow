namespace SimpleFlow.Services;

/// <summary>
/// Результат операции сохранения данных.
/// </summary>
public sealed record DatabaseOperationResult(bool Succeeded, string? ErrorMessage = null);
