namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// Defines the accessible attributes of a ToolbarContent
/// </summary>
public interface IToolbarContent
{
    /// <summary>
    /// Whether the item is inside the context menu rather than the primary toolbar.
    /// </summary>
    bool IsInMenu();
}
