using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Switch.Enums;

namespace ViciOne.Ui.Blazor.Components.Switch;

/// <summary>
/// Component for rendering a switch UI element
/// </summary>
public sealed partial class Switch : ComponentBase
{
    /// <summary>
    /// True when user interaction should be allowed, otherwise false
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// True when switch should be rendered in "on" state, otherwise false when it should be rendered in "off" state
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// Raised when <see cref="Value" /> has changed
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Size of the switch
    /// </summary>
    [Parameter]
    public SwitchSize Size { get; set; } = SwitchSize.Medium;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    private async Task ButtonClickAsync()
    {
        if (Enabled)
            await ToggleAsync();
    }

    private async Task ToggleAsync()
    {
        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(!Value);
    }
}

