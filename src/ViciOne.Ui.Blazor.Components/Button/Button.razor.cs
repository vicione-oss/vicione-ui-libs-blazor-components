using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Button;

/// <summary>
/// Component for rendering a button UI element with <see cref="Text">text</see> and <see cref="IconCssClass">optional icon</see>
/// </summary>
public sealed partial class Button : ComponentBase, IHasIcon
{
    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Content of the styles attribute of the button tag
    /// </summary>
    /// <remarks>
    /// https://www.w3.org/TR/css-style-attr/#intro
    /// </remarks>
    [Parameter]
    public string? CssInline { get; set; }

    /// <summary>
    /// True when user interaction should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#the-id-attribute">id</see> attribute
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Raised when the button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// Size of the button
    /// </summary>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Medium;

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
    /// Text displayed in the button
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    private async Task ButtonClickAsync(MouseEventArgs _)
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }
}
