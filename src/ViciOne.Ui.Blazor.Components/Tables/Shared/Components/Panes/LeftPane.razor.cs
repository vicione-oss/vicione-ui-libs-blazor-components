using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Panes;

/// <summary>
/// Renders arbitrary content in a <see href="https://en.wikipedia.org/wiki/Paned_window_(computing)">pane</see>
/// displayed on the left of the table.
/// </summary>
public sealed partial class LeftPane : ComponentBase
{
    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Renders the pane content
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
