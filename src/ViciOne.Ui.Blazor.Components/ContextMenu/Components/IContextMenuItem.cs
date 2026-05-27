namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

internal interface IContextMenuItem
{
    /// <returns>Context menu that is shown when the context menu item is clicked / focused / hovered</returns>
    IContextMenu? GetChildContextMenu();

    /// <summary>
    /// Closes the child context menu if it is open or pending to be shown.
    /// </summary>
    Task CloseChildContextMenuAsync();
}
