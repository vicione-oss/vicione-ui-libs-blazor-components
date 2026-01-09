using Microsoft.AspNetCore.Components.Web;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

/// <summary>
/// Context menu context
/// </summary>
public interface IContextMenuContext
{
    /// <inheritdoc cref="ContextMenuItemFilter"/>
    ContextMenuItemFilter? ItemFilter { get; }

    /// <summary>
    /// Arguments passed with the mouse event raised as a result of the right click associated with the context menu
    /// </summary>
    MouseEventArgs MouseEventArgs { get; }
}
