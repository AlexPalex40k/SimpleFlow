using SimpleFlow.Models;

namespace SimpleFlow.Tests;

/// <summary>
/// Тесты правил валидации товара.
/// </summary>
public class ProductValidationTests
{
    /// <summary>
    /// Корректность заполненной модели товара.
    /// </summary>
    [Fact]
    public void ValidProduct_HasNoValidationErrors()
    {
        var product = new Product
        {
            Sku = "MAT-001",
            Name = "Цемент",
            Description = "Мешок 25 кг",
            UnitPrice = 450.50m,
            UnitOfMeasure = "шт."
        };

        var results = ModelValidation.Validate(product);

        Assert.Empty(results);
    }

    /// <summary>
    /// Ошибки валидации при отсутствии обязательных полей товара.
    /// </summary>
    [Fact]
    public void EmptyRequiredFields_HaveValidationErrors()
    {
        var product = new Product
        {
            Sku = string.Empty,
            Name = string.Empty,
            UnitOfMeasure = string.Empty
        };

        var results = ModelValidation.Validate(product);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Product.Sku)));
        Assert.True(ModelValidation.HasErrorFor(results, nameof(Product.Name)));
        Assert.True(ModelValidation.HasErrorFor(results, nameof(Product.UnitOfMeasure)));
    }

    /// <summary>
    /// Ошибка валидации при слишком длинном складском коде товара.
    /// </summary>
    [Fact]
    public void SkuLongerThanMaximum_HasValidationError()
    {
        var product = new Product { Sku = new string('A', 51), Name = "Товар" };

        var results = ModelValidation.Validate(product);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Product.Sku)));
    }

    /// <summary>
    /// Ошибка валидации при отрицательной цене товара.
    /// </summary>
    [Fact]
    public void NegativeUnitPrice_HasValidationError()
    {
        var product = new Product { Sku = "MAT-001", Name = "Товар", UnitPrice = -0.01m };

        var results = ModelValidation.Validate(product);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Product.UnitPrice)));
    }
}
