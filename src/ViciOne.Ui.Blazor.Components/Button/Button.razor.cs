using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Button;

/// <summary>
/// Component for rendering a button UI element with <see cref="Text">text</see> and <see cref="IconCssClass">optional icon</see>
/// </summary>
public sealed partial class Button : ComponentBase, IHasIcon
{
    private static readonly string s_rippleModulePath =
        $"./_content/{typeof(Button).Assembly.GetName().Name}/material-web/ripple.js";

    private static readonly ConditionalWeakTable<IJSRuntime, Task> s_rippleModuleLoads = [];

    /// <summary>
    /// True while the action behind the button is running, otherwise false
    /// </summary>
    /// <remarks>
    /// A busy button keeps its place, size and text and stays focusable, and reports itself as busy and disabled to assistive technology.
    /// It ignores clicks, so <see cref="OnClick"/> is not raised. Use <see cref="Enabled"/> for a button that is permanently unavailable,
    /// which takes precedence: a button that is not enabled shows no busy state.
    /// </remarks>
    [Parameter]
    public bool Busy { get; set; }

    /// <summary>
    /// Way the button shows that it is <see cref="Busy">busy</see>
    /// </summary>
    [Parameter]
    public ButtonBusyIndication BusyIndication { get; set; } = ButtonBusyIndication.Sweep;

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

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<Button> Logger { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await s_rippleModuleLoads.GetValue(JsRuntime, jsRuntime => jsRuntime.ImportAsync(s_rippleModulePath, Logger));
    }

    private async Task ButtonClickAsync(MouseEventArgs _)
    {
        if (Busy)
            return;

        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }
}
