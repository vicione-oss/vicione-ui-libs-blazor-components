using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <summary>
/// Section rail button
/// </summary>
public sealed partial class SectionRailButton : ComponentBase, IHasIcon
{
    /// <summary>
    /// True when the buttn should be active, otherwise false
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute
    /// </summary>
    [Parameter, EditorRequired]
    public required string Title { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconData { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task AnchorClickAsync(MouseEventArgs e)
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(e);
    }
}
