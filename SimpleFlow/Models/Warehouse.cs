using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

public class Warehouse : EntityBase
{
    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
