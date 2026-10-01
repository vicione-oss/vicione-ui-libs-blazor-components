using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Chrome shared by every column filter editor placed in a column's <c>FilterEditor</c> slot: a titled header,
/// the editor's own input in the body, and the fixed Cancel/Apply buttons. The buttons are not configurable —
/// an editor only supplies the title, the body and what applying and canceling do.
/// </summary>
/// <remarks>
/// Clicks are stopped from bubbling out so interacting with the editor does not reach the column header.
/// Keyboard activation of the buttons is left to the browser; an editor that wants <c>Enter</c> in its body to
/// apply raises the same callback from its own input.
/// </remarks>
public sealed partial class FilterEditorFrame : ComponentBase
{
    /// <summary>
    /// Text rendered into the header, naming the kind of filter the editor offers.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <summary>
    /// Content of the body, the editor's own input.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Invoked when the user applies the filter through the Apply button.
    /// </summary>
    [Parameter]
    public EventCallback OnApply { get; set; }

    /// <summary>
    /// Invoked when the user cancels through the Cancel button.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    private Task ApplyAsync()
        => OnApply.InvokeAsync();

    private Task CancelAsync()
        => OnCancel.InvokeAsync();
}
