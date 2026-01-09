using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Switch.Enums;

namespace ViciOne.Ui.Blazor.Components.Switch.Extensions;

internal static class SwitchSizeExtensions
{
    public static string ToModifierCssClass(this SwitchSize size)
        => size.ToString().ToDashCase();
}
