using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleFlow.Models;

public class Product : EntityBase
{
    [Required, StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999999999999.99)]
    public decimal UnitPrice { get; set; }

    [Required, StringLength(20)]
    public string UnitOfMeasure { get; set; } = "pcs";

    public bool IsActive { get; set; } = true;
}
