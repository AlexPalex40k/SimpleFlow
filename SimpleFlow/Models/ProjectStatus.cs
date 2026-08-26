using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Models;

/// <summary>
/// Статус строительного проекта.
/// </summary>
public enum ProjectStatus
{
    /// <summary>Черновик проекта.</summary>
    Draft,

    /// <summary>Запланированный проект.</summary>
    Planned,

    /// <summary>Активный проект.</summary>
    Active,

    /// <summary>Приостановленный проект.</summary>
    [Display(Name = "On Hold")]
    OnHold,

    /// <summary>Завершённый проект.</summary>
    Completed,

    /// <summary>Отменённый проект.</summary>
    Cancelled
}
