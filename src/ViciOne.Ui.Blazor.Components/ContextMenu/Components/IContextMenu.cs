using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

internal interface IContextMenu
{
    /// <summary>
    /// Parent context menu
    /// </summary>
    IContextMenu? ParentContextMenu { get; }

    /// <summary>
    /// Element reference for use in connection with JS interop
    /// </summary>
    ElementReference ElementReference { get; }

    /// <summary>
    /// Items of the context menu
    /// </summary>
    IEnumerable<IContextMenuItem> Items { get; }

    /// <summary>
    /// True when context menu is visible, otherwise false
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Raised when <see cref="HideAsync"/> was called but before the context menu is actually hidden
    /// </summary>
    event Action? Hiding;

    /// <summary>
    /// Shows the context menu
    /// </summary>
    Task ShowAsync(MouseEventArgs mouseEventArgs, ContextMenuItemFilter? itemFilter);

    /// <summary>
    /// Hides the context menu
    /// </summary>
    Task HideAsync();

    /// <summary>
    /// Registers the given <paramref name="contextMenuItem"/>
    /// </summary>
    void RegisterContextMenuItem(IContextMenuItem contextMenuItem);

    /// <summary>
    /// Unregisters the given <paramref name="contextMenuItem"/>
    /// </summary>
    void UnregisterContextMenuItem(IContextMenuItem contextMenuItem);
}
