using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

public class Customer : EntityBase
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TaxNumber { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Phone, StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
