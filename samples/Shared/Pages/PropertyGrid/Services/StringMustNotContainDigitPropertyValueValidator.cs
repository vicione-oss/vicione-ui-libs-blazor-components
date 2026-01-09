using System.Text.RegularExpressions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace Shared.Pages.PropertyGrid.Services;

internal sealed partial class StringMustNotContainDigitPropertyValueValidator : IPropertyValueValidator<string>
{
    [GeneratedRegex(@"[0-9]", RegexOptions.CultureInvariant)]
    private partial Regex Matcher();

    public string? Validate(string value)
    {
        var containsInvalidCharacters = Matcher().IsMatch(value);
        if (containsInvalidCharacters)
            return "Must not contain digits";

        return null;
    }
}
