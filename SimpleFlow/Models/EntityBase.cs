namespace SimpleFlow.Models;

/// <summary>
/// Базовая сущность с идентификатором и датами аудита.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Уникальный идентификатор сущности.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата и время создания записи в формате UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Дата и время последнего изменения записи в формате UTC.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
