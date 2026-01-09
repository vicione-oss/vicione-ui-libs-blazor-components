namespace ViciOne.Ui.Blazor.Components.ContextMenu.Services;

/// <summary>
/// Settings for context menus composed via <see cref="Components.ContextMenu"/>
/// </summary>
public interface IContextMenuSettings
{
    /// <summary>
    /// True when calling <see cref="IContextMenuRequest{TContext}.SendAsync(TContext)"/> should result
    /// in display of the associated context menu composed via <see cref="Components.ContextMenu"/>,
    /// otherwise false when the native context menu of the browser should be displayed.
    /// </summary>
    bool UseCustomMenu { get; set; }
}
