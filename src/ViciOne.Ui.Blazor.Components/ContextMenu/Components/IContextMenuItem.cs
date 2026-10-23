using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Context menu item as seen by <see cref="ContextMenuCoordinator"/>, which decides when its child context menu is
/// shown or closed; the item itself only renders and talks to the browser
/// </summary>
internal interface IContextMenuItem
{
    /// <summary>
    /// Context menu that contains the item
    /// </summary>
    IContextMenu ParentContextMenu { get; }

    /// <summary>
    /// Context menu that is shown when the item is clicked / hovered, or null when the item has none
    /// </summary>
    IContextMenu? ChildContextMenu { get; }

    /// <summary>
    /// Item filter passed on to <see cref="ChildContextMenu"/> when it is shown
    /// </summary>
    ContextMenuItemFilter? ItemFilter { get; }

    /// <summary>
    /// Measures in the browser where <see cref="ChildContextMenu"/> is placed
    /// </summary>
    /// <returns>
    /// Position next to the item where <see cref="ChildContextMenu"/> fits into the viewport, or null when it
    /// cannot be calculated
    /// </returns>
    Task<ChildContextMenuPosition?> CalculateChildContextMenuPositionAsync();

    /// <summary>
    /// Starts reporting to <see cref="ContextMenuCoordinator.MouseLeftItemAsync"/> whenever the mouse pointer
    /// leaves the item
    /// </summary>
    Task StartObserveMouseLeaveAsync();

    /// <summary>
    /// Stops what <see cref="StartObserveMouseLeaveAsync"/> started
    /// </summary>
    Task EndObserveMouseLeaveAsync();
}
