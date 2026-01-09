using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;

namespace ViciOne.Ui.Blazor.Components.Sidebar.Extensions;

internal static class SidebarPlacementExtensions
{
    /// <summary>
    /// Returns the CSS modifier class for the given sidebar <paramref name="placement"/>.
    /// </summary>
    /// <param name="placement">The sidebar placement value.</param>
    /// <returns>
    /// A class name in the form sidebar--{placement}, e.g., sidebar--left or sidebar--right.
    /// </returns>
    public static string ToModifierCssClass(this SidebarPlacement placement)
        => $"sidebar--{placement.ToString().ToDashCase()}";
}
