using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Popup.Services;

namespace ViciOne.Ui.Blazor.Components.Popup.Components;

/// <summary>
/// Component for rendering <see cref="PopupCell"/> components
/// </summary>
/// <remarks>
/// The component can be used only once because only a single popup root is currently supported.
/// Placing it at a central place like the MainLayout of your application matches this requirement.
/// </remarks>
public sealed partial class PopupRoot : ComponentBase, IDisposable
{
    [Inject]
    private IPopupRegistry PopupRegistry { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
        => PopupRegistry.Changed += PopupRegistryChanged;

    /// <inheritdoc/>
    public void Dispose()
        => PopupRegistry.Changed -= PopupRegistryChanged;

    private void PopupRegistryChanged()
        => InvokeAsync(StateHasChanged);
}
