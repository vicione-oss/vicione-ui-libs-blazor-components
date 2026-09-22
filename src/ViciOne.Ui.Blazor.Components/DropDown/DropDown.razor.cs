using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.DropDown;

/// <summary>
/// Component for rendering a drop-down element.
/// </summary>
/// <remarks>
/// Do not use this component in third party code.
/// It is intended for internal use only.
/// </remarks>
public sealed partial class DropDown<TItem> : ComponentBase, IAsyncDisposable
{
    private ElementReference _containerElement;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _jsInstance;
    private bool _visible;
    private bool _placementApplied;
    private bool _disposedAsync;

    /// <summary>
    /// Items in the drop-down
    /// </summary>
    [Parameter, EditorRequired]
    public required IReadOnlyList<TItem> Items { get; set; }

    /// <summary>
    /// Template used to render the content of a drop-down item
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<TItem> ItemTemplate { get; set; }

    /// <summary>
    /// True when multiple items can be selected, otherwise false
    /// </summary>
    [Parameter]
    public bool MultiSelection { get; set; }

    /// <summary>
    /// Selected items in the drop-down
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> SelectedItems { get; set; } = [];

    /// <summary>
    /// Raised when the <see cref="SelectedItems" /> has changed
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Used to receive events from a key source element to control drop-down functionality
    /// </summary>
    [Parameter, EditorRequired]
    public ElementReference? InputElementReference { get; set; }

    /// <summary>
    /// Selector for the title property of <typeparamref name="TItem"/>
    /// </summary>
    [Parameter]
    public Func<TItem, string>? TitleSelector { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ILogger<DropDown<TItem>> Logger { get; set; } = default!;

    private static string NoMatchText => Localization.DropDown.NoMatch;

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
        => await AttachInputElementIfNeededAsync();

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposedAsync)
            return;

        if (firstRender)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(DropDown<TItem>).Assembly.GetName().Name}/drop-down/drop-down.js");
        }

        if (!_disposedAsync && _jsModule is not null && _jsInstance is null)
        {
            _jsInstance = await _jsModule.InvokeConstructorAsync("DropDown", Logger, _containerElement);

            if (!_disposedAsync && _jsInstance is not null)
                await _jsInstance.InvokeVoidAsync("setMinimumWidth");
        }

        await AttachInputElementIfNeededAsync();
        await UpdatePlacementIfNeededAsync();
    }

    private async Task AttachInputElementIfNeededAsync()
    {
        if (_disposedAsync || _jsInstance is null || InputElementReference is not { } inputElement)
            return;

        await _jsInstance.InvokeVoidAsync("attachInputElement", inputElement);
    }

    private async Task UpdatePlacementIfNeededAsync()
    {
        if (_disposedAsync || _jsInstance is null)
            return;

        if (!_visible)
        {
            if (_placementApplied)
            {
                _placementApplied = false;
                await _jsInstance.InvokeVoidAsync("resetPlacement");
            }

            return;
        }

        await _jsInstance.InvokeVoidAsync("updatePlacement");
        _placementApplied = true;
    }

    /// <summary>
    /// Shows the drop-down
    /// </summary>
    public async Task ShowAsync()
    {
        _visible = true;

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Hides the drop-down
    /// </summary>
    public async Task HideAsync()
    {
        _visible = false;

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectItemAsync(TItem item)
    {
        List<TItem> selectedItems = [.. SelectedItems];

        if (MultiSelection)
        {
            if (!selectedItems.Remove(item))
                selectedItems.Add(item);
        }
        else
        {
            selectedItems = [item];
        }

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private bool IsSelected(TItem item)
        => SelectedItems.Contains(item);

    private bool IsMostRecentlySelected(TItem item)
        => SelectedItems.TakeLast(1).Contains(item);

    private async Task DisposeJsInstanceAsync()
    {
        if (_jsInstance is not null)
        {
            try
            {
                await _jsInstance.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                InvokingJsInstanceDisposeFailed(Logger, ex);
            }

            try
            {
                await _jsInstance.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                DisposingJsInstanceFailed(Logger, ex);
            }

            _jsInstance = null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await DisposeJsInstanceAsync();

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            _jsModule = null;
        }

        _disposedAsync = true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsInstance.dispose() failed")]
    private static partial void InvokingJsInstanceDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsInstance failed")]
    private static partial void DisposingJsInstanceFailed(ILogger logger, Exception ex);
}
