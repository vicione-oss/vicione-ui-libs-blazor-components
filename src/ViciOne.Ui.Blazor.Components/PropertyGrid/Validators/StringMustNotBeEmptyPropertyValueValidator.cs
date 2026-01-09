using ViciOne.Ui.Blazor.Components.Resources.PropertyGrid.Localization;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

/// <summary>
/// Validates a given string for not being <see langword="null"/> or whitespace.
/// </summary>
public sealed class StringMustNotBeEmptyPropertyValueValidator : IPropertyValueValidator<string?>
{
    /// <inheritdoc/>
    public string? Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationMessages.StringMustNotBeEmpty;

        return null;
    }
}
