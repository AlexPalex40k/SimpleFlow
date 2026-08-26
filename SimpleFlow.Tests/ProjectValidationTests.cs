using SimpleFlow.Models;

namespace SimpleFlow.Tests;

/// <summary>
/// Тесты правил валидации строительного проекта.
/// </summary>
public class ProjectValidationTests
{
    /// <summary>
    /// Корректность заполненной модели проекта.
    /// </summary>
    [Fact]
    public void ValidProject_HasNoValidationErrors()
    {
        var project = new Project
        {
            ProjectNumber = "PRJ-001",
            Name = "Business center",
            CustomerId = 1,
            Status = ProjectStatus.Planned,
            ContractAmount = 1_000_000m
        };

        var results = ModelValidation.Validate(project);

        Assert.Empty(results);
    }

    /// <summary>
    /// Ошибки валидации при отсутствии обязательных данных проекта.
    /// </summary>
    [Fact]
    public void EmptyRequiredFields_HaveValidationErrors()
    {
        var project = new Project();

        var results = ModelValidation.Validate(project);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Project.ProjectNumber)));
        Assert.True(ModelValidation.HasErrorFor(results, nameof(Project.Name)));
        Assert.True(ModelValidation.HasErrorFor(results, nameof(Project.CustomerId)));
    }

    /// <summary>
    /// Ошибка валидации при слишком длинном номере проекта.
    /// </summary>
    [Fact]
    public void ProjectNumberLongerThanMaximum_HasValidationError()
    {
        var project = new Project
        {
            ProjectNumber = new string('A', 31),
            Name = "Project",
            CustomerId = 1
        };

        var results = ModelValidation.Validate(project);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Project.ProjectNumber)));
    }

    /// <summary>
    /// Ошибка валидации при отрицательной сумме договора.
    /// </summary>
    [Fact]
    public void NegativeContractAmount_HasValidationError()
    {
        var project = new Project
        {
            ProjectNumber = "PRJ-001",
            Name = "Project",
            CustomerId = 1,
            ContractAmount = -0.01m
        };

        var results = ModelValidation.Validate(project);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Project.ContractAmount)));
    }
}
