using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Components;

/// <summary>
/// Component for rendering a loading spinner message
/// </summary>
public sealed partial class LoadingSpinnerMessage : ComponentBase
{
    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// <see langword="true"/> when the message should be visible, otherwise <see langword="false"/>
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Renders the message
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment? ChildContent { get; set; }
}
