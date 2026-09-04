using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Resizeable.Enums;
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

    /// <summary>
    /// Returns the position the resize handle takes for the given sidebar <paramref name="placement"/>.
    /// </summary>
    /// <param name="placement">The sidebar placement value.</param>
    /// <returns>
    /// The position of the edge the sidebar is widened from, so <see cref="ResizeHandlePosition.Right"/>
    /// for a sidebar placed on the left and <see cref="ResizeHandlePosition.Left"/> for one placed on the right.
    /// </returns>
    public static ResizeHandlePosition ToResizeHandlePosition(this SidebarPlacement placement)
        => placement == SidebarPlacement.Left ? ResizeHandlePosition.Right : ResizeHandlePosition.Left;
}
