using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components;

/// <summary>
/// Tooltip for a property editor
/// </summary>
public sealed partial class PropertyEditorTooltip : ComponentBase
{
    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter] public string? CssClass { get; set; }

    /// <summary>
    /// Content displayed in the tooltip
    /// </summary>
    [Parameter, EditorRequired] public RenderFragment Content { get; set; }

    /// <summary>
    /// Optional content displayed after <see cref="Content"/> separated by a horizontal line
    /// </summary>
    [Parameter] public RenderFragment? AfterContent { get; set; }

    /// <summary>
    /// <see langword="true"/> when a close button should be rendered next to the tooltip message,
    /// </summary>
    [Parameter] public bool HasCloseButton { get; set; }

    /// <summary>
    /// Event callback invoked when the tooltip close button is clicked
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task CloseButtonClickAsync()
    {
        if (OnClose.HasDelegate)
            await OnClose.InvokeAsync();
    }
}
