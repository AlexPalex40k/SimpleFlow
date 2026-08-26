using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleFlow.Models;

/// <summary>
/// Строительный проект компании.
/// </summary>
public class Project : EntityBase
{
    /// <summary>Уникальный номер проекта.</summary>
    [Required, StringLength(30)]
    public string ProjectNumber { get; set; } = string.Empty;

    /// <summary>Наименование проекта.</summary>
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Идентификатор клиента проекта.</summary>
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    /// <summary>Клиент проекта.</summary>
    public Customer? Customer { get; set; }

    /// <summary>Текущий статус проекта.</summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    /// <summary>Дата начала проекта.</summary>
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    /// <summary>Плановая дата завершения проекта.</summary>
    [DataType(DataType.Date)]
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>Фактическая дата завершения проекта.</summary>
    [DataType(DataType.Date)]
    public DateTime? ActualEndDate { get; set; }

    /// <summary>Адрес строительного объекта.</summary>
    [StringLength(300)]
    public string? Address { get; set; }

    /// <summary>Сумма договора по проекту.</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 9999999999999999.99)]
    public decimal ContractAmount { get; set; }

    /// <summary>Имя руководителя проекта.</summary>
    [StringLength(150)]
    public string? ProjectManager { get; set; }

    /// <summary>Описание проекта.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }
}
