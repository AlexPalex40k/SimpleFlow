using SimpleFlow.Models;

namespace SimpleFlow.Tests;

/// <summary>
/// Тесты правил валидации склада.
/// </summary>
public class WarehouseValidationTests
{
    /// <summary>
    /// Корректность заполненной модели склада.
    /// </summary>
    [Fact]
    public void ValidWarehouse_HasNoValidationErrors()
    {
        var warehouse = new Warehouse
        {
            Code = "MAIN",
            Name = "Основной склад",
            Address = "Москва"
        };

        var results = ModelValidation.Validate(warehouse);

        Assert.Empty(results);
    }

    /// <summary>
    /// Ошибки валидации при отсутствии обязательных полей склада.
    /// </summary>
    [Fact]
    public void EmptyRequiredFields_HaveValidationErrors()
    {
        var warehouse = new Warehouse { Code = string.Empty, Name = string.Empty };

        var results = ModelValidation.Validate(warehouse);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Warehouse.Code)));
        Assert.True(ModelValidation.HasErrorFor(results, nameof(Warehouse.Name)));
    }

    /// <summary>
    /// Ошибка валидации при слишком длинном коде склада.
    /// </summary>
    [Fact]
    public void CodeLongerThanMaximum_HasValidationError()
    {
        var warehouse = new Warehouse { Code = new string('A', 31), Name = "Склад" };

        var results = ModelValidation.Validate(warehouse);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Warehouse.Code)));
    }

    /// <summary>
    /// Ошибка валидации при слишком длинном адресе склада.
    /// </summary>
    [Fact]
    public void AddressLongerThanMaximum_HasValidationError()
    {
        var warehouse = new Warehouse
        {
            Code = "MAIN",
            Name = "Склад",
            Address = new string('A', 301)
        };

        var results = ModelValidation.Validate(warehouse);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Warehouse.Address)));
    }
}
