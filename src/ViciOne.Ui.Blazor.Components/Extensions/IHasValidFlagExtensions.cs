using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static class IHasValidFlagExtensions
{
    /// <summary>
    /// Adds "invalid" to the given <paramref name="cssClasses"/> when <see cref="IHasValidFlag.Valid"/>
    /// implemented by <paramref name="component"/> is true
    /// </summary>
    public static void WithInputValidationStyling(this IHasValidFlag component, ICollection<string> cssClasses)
    {
        if (component.Valid is null)
            return;

        if (!component.Valid.Value)
            cssClasses.Add("invalid");
    }
}
