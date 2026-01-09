using ViciOne.Ui.Blazor.Components.ContextMenu.Components;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

/// <summary>
/// Context menu item filter
/// </summary>
public class ContextMenuItemFilter
{
    /// <summary>
    /// Types that represent the case of application for items in the context menu
    /// </summary>
    /// <remarks>
    /// The list is compared with <see cref="ContextMenuItemBase.ApplicableTo"/> to find
    /// items that should be displayed or hidden
    /// </remarks>
    public IEnumerable<Type>? ApplicableTo { get; set; }
}
