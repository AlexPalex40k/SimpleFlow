namespace SimpleFlow.Models;

public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
