using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

/// <summary>
/// Клиент компании.
/// </summary>
public class Customer : EntityBase
{
    [Required, StringLength(150)]
    /// <summary>
    /// Имя или наименование клиента.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    /// <summary>
    /// Налоговый номер клиента.
    /// </summary>
    public string? TaxNumber { get; set; }

    [EmailAddress, StringLength(254)]
    /// <summary>
    /// Адрес электронной почты клиента.
    /// </summary>
    public string? Email { get; set; }

    [Phone, StringLength(50)]
    /// <summary>
    /// Номер телефона клиента.
    /// </summary>
    public string? Phone { get; set; }

    [StringLength(300)]
    /// <summary>
    /// Почтовый адрес клиента.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Признак активности клиента.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
