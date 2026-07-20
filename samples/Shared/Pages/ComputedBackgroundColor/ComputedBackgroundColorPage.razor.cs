using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Shared.Pages.ComputedBackgroundColor;

public sealed partial class ComputedBackgroundColorPage : ComponentBase, IAsyncDisposable
{
    private readonly ElementReference[] _innerElements = new ElementReference[8];
    private IJSObjectReference? _jsModule;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import",
            "./_content/Shared/js/computed-background-color-page.js");

        foreach (var innerElement in _innerElements)
            await _jsModule.InvokeVoidAsync("applyComputedBackgroundColor", innerElement);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
            await _jsModule.DisposeAsync();
    }
}
