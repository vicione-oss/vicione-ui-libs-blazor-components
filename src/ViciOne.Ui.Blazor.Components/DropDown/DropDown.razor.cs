using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using DropDownLocalization = ViciOne.Ui.Blazor.Components.Resources.DropDown.Localization.DropDown;

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
    private IJSObjectReference? _jsAttachResult;
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

    private static string NoMatchText => DropDownLocalization.NoMatch;

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

        if (!_disposedAsync && _jsModule is not null && _jsAttachResult is null)
        {
            _jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", _containerElement);

            if (!_disposedAsync && _jsAttachResult is not null)
                await _jsAttachResult.InvokeVoidAsync("setMinimumWidth");
        }

        await AttachInputElementIfNeededAsync();
        await UpdatePlacementIfNeededAsync();
    }

    private async Task AttachInputElementIfNeededAsync()
    {
        if (_disposedAsync || _jsAttachResult is null || InputElementReference is not { } inputElement)
            return;

        await _jsAttachResult.InvokeVoidAsync("attachInputElement", inputElement);
    }

    private async Task UpdatePlacementIfNeededAsync()
    {
        if (_disposedAsync || _jsAttachResult is null)
            return;

        if (!_visible)
        {
            if (_placementApplied)
            {
                _placementApplied = false;
                await _jsAttachResult.InvokeVoidAsync("resetPlacement");
            }

            return;
        }

        await _jsAttachResult.InvokeVoidAsync("updatePlacement");
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

    private async Task DisposeJsAttachResultAsync()
    {
        if (_jsAttachResult is not null)
        {
            try
            {
                await _jsAttachResult.InvokeVoidAsync("dispose");
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                InvokingJsAttachResultDisposeFailed(Logger, ex);
            }

            try
            {
                await _jsAttachResult.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
            }
            catch (Exception ex)
            {
                DisposingJsAttachResultFailed(Logger, ex);
            }

            _jsAttachResult = null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await DisposeJsAttachResultAsync();

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

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsAttachResult.dispose() failed")]
    private static partial void InvokingJsAttachResultDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    private static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);
}
