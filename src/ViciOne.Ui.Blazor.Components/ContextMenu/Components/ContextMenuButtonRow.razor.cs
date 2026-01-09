using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Component for rendering a context menu buttons in a dedicated row
/// </summary>
public sealed partial class ContextMenuButtonRow : ContextMenuItemBase
{
    private readonly List<ContextMenuButton> _buttons = [];

    /// <summary>
    /// Content of the button row
    /// </summary>
    /// <remarks>
    /// Use component <see cref="ContextMenuButton"/> to render a button
    /// </remarks>
    [Parameter]
    public RenderFragment<ContextMenuButtonRow>? ChildContent { get; set; }

    /// <summary>
    /// True when buttons should be enabled by default, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Optional title of the row
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// True when a separator for visual grouping should be rendered, otherwise false
    /// </summary>
    [Parameter]
    public bool BeginGroup { get; set; }

    internal void AddButton(ContextMenuButton button)
    {
        if (!_buttons.Contains(button))
        {
            _buttons.Add(button);
            StateHasChanged();
        }
    }

    internal void RemoveButton(ContextMenuButton button)
    {
        var removed = _buttons.Remove(button);
        if (removed)
            StateHasChanged();
    }
}
