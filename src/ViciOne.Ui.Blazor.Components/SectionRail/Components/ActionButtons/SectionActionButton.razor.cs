using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components.ActionButtons;

/// <summary>
/// Component for rendering a button in render fragment <see cref="SectionLayout.ActionButtons"/>
/// </summary>
public sealed partial class SectionActionButton : ComponentBase, IHasIcon
{
    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// True when user interaction should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public string? IconData { get; set; }

    /// <summary>
    /// Raised when the button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    private async Task ButtonClickAsync(MouseEventArgs args)
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(args);
    }
}
