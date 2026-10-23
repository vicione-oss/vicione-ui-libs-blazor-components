using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

internal interface IContextMenu
{
    /// <summary>
    /// Element reference for use in connection with JS interop
    /// </summary>
    ElementReference ElementReference { get; }

    /// <summary>
    /// True when context menu is visible, otherwise false
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Shows the context menu
    /// </summary>
    Task ShowAsync(MouseEventArgs mouseEventArgs, ContextMenuItemFilter? itemFilter);

    /// <summary>
    /// Hides the context menu together with all of its child context menus
    /// </summary>
    Task CloseAsync();
}
