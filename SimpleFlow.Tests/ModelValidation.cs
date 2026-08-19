using System.ComponentModel.DataAnnotations;

namespace SimpleFlow.Tests;

internal static class ModelValidation
{
    internal static IReadOnlyList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results;
    }

    internal static bool HasErrorFor(
        IEnumerable<ValidationResult> results,
        string propertyName)
    {
        return results.Any(result => result.MemberNames.Contains(propertyName));
    }
}
