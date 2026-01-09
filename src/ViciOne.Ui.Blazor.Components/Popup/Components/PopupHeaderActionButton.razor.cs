using Microsoft.AspNetCore.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that renders a popup action button for use within popup headers.
/// </summary>
public sealed partial class PopupHeaderActionButton : ComponentBase
{
    [CascadingParameter]
    private MonochromeIconSize IconSize { get; set; }

    /// <summary>
    /// Name of the icon to display in the button.
    /// </summary>
    [Parameter, EditorRequired]
    public MonochromeIconName IconName { get; set; }

    /// <summary>
    /// Raised when the button was clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task ButtonClickAsync()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }
}
