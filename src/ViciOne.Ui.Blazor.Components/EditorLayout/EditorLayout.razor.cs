using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.EditorLayout;

/// <summary>
/// Component for rendering an editor layout
/// </summary>
public sealed partial class EditorLayout : ComponentBase
{
    /// <summary>
    /// Renders the left part of the editor layout
    /// </summary>
    [Parameter]
    public RenderFragment? Left { get; set; }

    /// <summary>
    /// Renders the top part of the editor layout
    /// </summary>
    [Parameter]
    public RenderFragment? Top { get; set; }

    /// <summary>
    /// Renders the center part of the editor layout
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment Center { get; set; }

    /// <summary>
    /// Renders the bottom part of the editor layout
    /// </summary>
    [Parameter]
    public RenderFragment? Bottom { get; set; }

    /// <summary>
    /// Renders the right part of the editor layout
    /// </summary>
    [Parameter]
    public RenderFragment? Right { get; set; }

    /// <summary>
    /// Renders the debug part of the editor layout
    /// </summary>
    [Parameter]
    public RenderFragment? Debug { get; set; }
}
