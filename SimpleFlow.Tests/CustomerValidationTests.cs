using SimpleFlow.Models;

namespace SimpleFlow.Tests;

/// <summary>
/// Тесты правил валидации клиента.
/// </summary>
public class CustomerValidationTests
{
    /// <summary>
    /// Корректность заполненной модели клиента.
    /// </summary>
    [Fact]
    public void ValidCustomer_HasNoValidationErrors()
    {
        var customer = new Customer
        {
            Name = "СтройИнвест",
            TaxNumber = "1234567890",
            Email = "office@example.com",
            Phone = "+7 999 123-45-67",
            Address = "Москва"
        };

        var results = ModelValidation.Validate(customer);

        Assert.Empty(results);
    }

    /// <summary>
    /// Ошибка валидации при отсутствии имени клиента.
    /// </summary>
    [Fact]
    public void EmptyName_HasValidationError()
    {
        var customer = new Customer { Name = string.Empty };

        var results = ModelValidation.Validate(customer);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Customer.Name)));
    }

    /// <summary>
    /// Ошибка валидации при слишком длинном имени клиента.
    /// </summary>
    [Fact]
    public void NameLongerThanMaximum_HasValidationError()
    {
        var customer = new Customer { Name = new string('A', 151) };

        var results = ModelValidation.Validate(customer);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Customer.Name)));
    }

    /// <summary>
    /// Ошибка валидации при некорректном адресе электронной почты.
    /// </summary>
    [Fact]
    public void InvalidEmail_HasValidationError()
    {
        var customer = new Customer { Name = "Клиент", Email = "invalid-email" };

        var results = ModelValidation.Validate(customer);

        Assert.True(ModelValidation.HasErrorFor(results, nameof(Customer.Email)));
    }
}
