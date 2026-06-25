using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Extensions;

internal static class TabSizeExtensions
{
    public static string ToModifierCssClass(this TabSize size)
        => size.ToString().ToDashCase();
}
