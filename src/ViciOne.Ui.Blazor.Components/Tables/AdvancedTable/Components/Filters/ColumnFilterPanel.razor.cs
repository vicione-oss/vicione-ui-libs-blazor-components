using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Hosts a column's filter editor in a panel anchored under <see cref="AnchorElement"/>.
/// Uses <c>position: fixed</c> with JavaScript-calculated coordinates to escape the table's overflow
/// container, pulls itself back inside its containing block, moves focus into its first input, and requests a close
/// on outside click, scroll or resize. Exists only while its owner keeps the panel open, so tearing the
/// close observer down is part of disposal.
/// </summary>
public sealed partial class ColumnFilterPanel : ComponentBase, IAsyncDisposable
{
    private bool _positioned;
    private double _panelLeft;
    private double _panelTop;
    private ElementReference _panelElement;
    private DotNetObjectReference<ColumnFilterPanel>? _dotNetObjectReference;
    private int _closeObserverId = -1;
    private bool _registeringCloseObserver;
    private bool _disposedAsync;

    private string PanelStyle
        => $"left: {_panelLeft.ToAttributeValue(precision: 3)}px; top: {_panelTop.ToAttributeValue(precision: 3)}px;";

    /// <summary>
    /// The element the panel is positioned under. Also excluded from outside-click detection, so clicking it
    /// again toggles rather than closing twice.
    /// </summary>
    [Parameter, EditorRequired]
    public required ElementReference AnchorElement { get; set; }

    [CascadingParameter]
    private IAdvancedTableJsSession TableJsSession { get; set; } = default!;

    /// <summary>
    /// The panel's content, normally a column's filter editor.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Raised when the user dismisses the panel by clicking outside it, scrolling or resizing. The owner is
    /// expected to stop rendering the panel in response — without a handler the panel can no longer be
    /// dismissed, because JavaScript drops its listeners before reporting the dismissal.
    /// </summary>
    [Parameter, EditorRequired]
    public required EventCallback OnCloseRequested { get; set; }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            await PositionAndObserveAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit is gone; nothing to position or observe against anymore.
        }
        catch (ObjectDisposedException)
        {
            // The table may dispose its module first — parent and child disposal are not ordered. Nothing
            // further can be reached through it.
        }
    }

    private async Task PositionAndObserveAsync()
    {
        if (_disposedAsync)
            return;

        var tableJsModule = await TableJsSession.GetModuleAsync();

        if (!_positioned)
        {
            var position = await tableJsModule.InvokeAsync<PanelPosition>("getFilterPanelPosition", AnchorElement);

            _panelLeft = position.Left;
            _panelTop = position.Bottom;
            _positioned = true;

            StateHasChanged();

            return;
        }

        if (_closeObserverId < 0 && !_registeringCloseObserver && !string.IsNullOrEmpty(_panelElement.Id))
            await FinalizePanelAsync(tableJsModule);
    }

    // Runs once per open, after the panel content is in the DOM: pull it back inside its containing block, move focus
    // into it, and start watching for the interactions that dismiss it.
    private async Task FinalizePanelAsync(IJSObjectReference tableJsModule)
    {
        // Guard set synchronously before the awaits: OnAfterRenderAsync can re-enter (extra render
        // cycles from a filter apply/refresh), and _closeObserverId is only assigned after the interop
        // completes. Without this, a re-entrant call would pass the `< 0` check and register a second
        // observer whose id is never stored — a listener bound to the panel that nothing ever stops.
        _registeringCloseObserver = true;

        try
        {
            await ClampPanelIntoContainingBlockAsync(tableJsModule);

            if (_disposedAsync)
                return;

            // Focus the first focusable element now that the panel is positioned and its content is in the DOM.
            await tableJsModule.InvokeVoidAsync("focusFirstFocusable", _panelElement);

            await StartCloseObserverAsync(tableJsModule);
        }
        finally
        {
            _registeringCloseObserver = false;
        }
    }

    private async Task ClampPanelIntoContainingBlockAsync(IJSObjectReference tableJsModule)
    {
        var clampedLeft = await tableJsModule.InvokeAsync<double>("clampPanelLeft", _panelElement, _panelLeft);
        var clampedTop = await tableJsModule.InvokeAsync<double>("clampPanelTop", _panelElement, _panelTop);

        if (clampedLeft == _panelLeft && clampedTop == _panelTop)
            return;

        _panelLeft = clampedLeft;
        _panelTop = clampedTop;

        StateHasChanged();
    }

    private async Task StartCloseObserverAsync(IJSObjectReference tableJsModule)
    {
        if (_disposedAsync)
            return;

        _dotNetObjectReference ??= DotNetObjectReference.Create(this);

        var observerId = await tableJsModule.InvokeAsync<int>("startObserveClose", _panelElement, AnchorElement,
            _dotNetObjectReference);

        // The component may have been disposed while the interop was in flight. Tear the observer down at
        // once rather than leaving a listener bound to a panel that is gone, holding a reference to a
        // component nobody will call StopObserveClose for.
        if (!_disposedAsync)
        {
            _closeObserverId = observerId;

            return;
        }

        await tableJsModule.InvokeVoidAsync("stopObserveClose", observerId);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _disposedAsync = true;

        try
        {
            if (_closeObserverId >= 0)
            {
                var module = await TableJsSession.GetModuleAsync();

                await module.InvokeVoidAsync("stopObserveClose", _closeObserverId);
            }
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
        }
        catch (ObjectDisposedException)
        {
            // The table disposed the module first. Blazor does not order parent and child disposal, so this
            // is reachable whenever the whole table goes away with the panel still open.
        }

        _dotNetObjectReference?.Dispose();
    }

    /// <summary>
    /// Called from JavaScript when the panel should close due to an outside click, scroll, or resize.
    /// </summary>
    [JSInvokable]
    public async Task ClosePanelAsync()
    {
        if (_disposedAsync)
            return;

        // JavaScript removes its own listeners and drops the registry entry before calling back, so there is
        // nothing left to stop — clearing the id keeps disposal from making a pointless interop round-trip.
        _closeObserverId = -1;

        await InvokeAsync(OnCloseRequested.InvokeAsync);
    }
}
