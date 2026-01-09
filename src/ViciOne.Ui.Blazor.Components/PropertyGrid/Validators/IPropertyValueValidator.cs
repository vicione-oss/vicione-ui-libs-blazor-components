namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

/// <summary>
/// Validator for property values of type <typeparamref name="TPropertyValue"/>
/// </summary>
public interface IPropertyValueValidator<in TPropertyValue>
{
    /// <summary>
    /// Validates the given <paramref name="value"/> and returns a meaningful error message when validation fails,
    /// otherwise returns <see langword="null"/>.
    /// </summary>
    string? Validate(TPropertyValue value);
}
