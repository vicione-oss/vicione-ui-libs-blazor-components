using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// A component that renders a close action button for use within popup headers.
/// </summary>
public sealed partial class PopupHeaderCloseActionButton : ComponentBase
{
    [CascadingParameter]
    private IPopup Popup { get; set; } = default!;

    private async Task ButtonClickAsync()
        => await Popup.CloseAsync();
}
