using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Component for rendering a context menu header
/// </summary>
public sealed partial class ContextMenuHeader : ContextMenuItemBase
{
    /// <summary>
    /// Text displayed in the header
    /// </summary>
    [Parameter, EditorRequired] public required string Text { get; set; }
}
