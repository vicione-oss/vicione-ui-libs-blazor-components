using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Button.Extensions;

/// <summary>
/// Extension methods for <see cref="ButtonBusyIndication"/>.
/// </summary>
/// <remarks>
/// Provides the mapping to the CSS modifier class used by the UI theme.
/// </remarks>
public static class ButtonBusyIndicationExtensions
{
    /// <summary>
    /// Returns the CSS modifier classes for the given <paramref name="busyIndication"/> in dash-case form.
    /// </summary>
    /// <param name="busyIndication">The way the button shows that it is busy.</param>
    /// <returns>
    /// One class per effect, each in the form <c>button--busy-{effect}</c>, e.g.
    /// <c>button--busy-sweep</c> and <c>button--busy-spinning-icon</c>.
    /// An uninitialized <paramref name="busyIndication"/> maps to the classes of the default value.
    /// </returns>
    internal static IEnumerable<string> ToModifierCssClasses(this ButtonBusyIndication busyIndication)
    {
        var name = busyIndication.GetName();

        if (string.IsNullOrEmpty(name))
            name = ButtonBusyIndication.GetDefaultValue().GetName();

        if (name == nameof(ButtonBusyIndication.SweepAndSpinningIcon))
        {
            return
            [
                ToModifierCssClass(nameof(ButtonBusyIndication.Sweep)),
                ToModifierCssClass(nameof(ButtonBusyIndication.SpinningIcon)),
            ];
        }

        return [ToModifierCssClass(name)];
    }

    private static string ToModifierCssClass(string busyIndicationName)
        => $"button--busy-{busyIndicationName.ToDashCase()}";
}
