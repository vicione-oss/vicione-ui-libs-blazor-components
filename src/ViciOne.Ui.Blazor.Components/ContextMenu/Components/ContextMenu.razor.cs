using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Popup.Components;
using ViciOne.Ui.Blazor.Components.Popup.Services;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Component for rendering a context menu
/// </summary>
public sealed partial class ContextMenu : ComponentBase, IContextMenu, IPopup, IDisposable, IAsyncDisposable
{
    private ContextMenuCoordinator? _coordinator;
    private bool _isChildContextMenu;
    private ContextMenuItemFilter? _itemFilter;
    private int _isObservingWindowPointerDown;
    private ContextMenuPosition? _position;
    private bool _shouldRender = true;
    private bool _visible;

    // Visibility the most recent render put on screen, and the visibility last reported by VisibilityChanged.
    private bool _renderedVisible;
    private bool _reportedVisible;

    // Every ShowAsync() and CloseAsync() takes the next version, so work still in flight for an older call can
    // tell that it has been replaced and must not touch the menu anymore.
    private int _requestVersion;

    // Version of the request the most recent render was built for; see ContextMenu.razor.
    private int _renderedRequestVersion;

    private PendingShow? _pendingShow;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ContextMenu>? _dotNetObjectReference;
    private IJSObjectReference? _jsInstance;
    private Task? _attachJsTask;

    private bool _disposed;
    private bool _disposedAsync;

    private ElementReference _elementReference;

    /// <inheritdoc/>
    public ElementReference ElementReference => _elementReference;

    /// <inheritdoc/>
    public bool Visible => _visible;

    /// <summary>
    /// Coordinator of the root context menu, provided to child context menus and their items
    /// </summary>
    [CascadingParameter]
    internal ContextMenuCoordinator? InheritedCoordinator { get; set; }

    /// <inheritdoc cref="ContextMenuApplicableTo"/>
    [Parameter]
    public ContextMenuApplicableTo? ApplicableTo { get; set; }

    /// <summary>
    /// Content of the context menu
    /// </summary>
    /// <remarks>
    /// Use component <see cref="ContextMenuHeader"/>, <see cref="ContextMenuButtonRow"/> and / or
    /// <see cref="ContextMenuItem"/> to render well-defined sections of content
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Raised when <see cref="Visible"/> has changed
    /// </summary>
    /// <remarks>
    /// Raised once the context menu is shown or hidden on screen. A context menu closed before it appeared on
    /// screen raises nothing.
    /// </remarks>
    [Parameter]
    public EventCallback<bool> VisibilityChanged { get; set; }

    [Inject]
    private IPopupRegistry PopupRegistry { get; set; } = default!;

    [Inject]
    private ILogger<ContextMenu> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// Raised when <see cref="CloseAsync"/> is called, before the context menu and its child context menus are hidden
    /// </summary>
    internal event Action? Closing;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _isChildContextMenu = InheritedCoordinator is not null;
        _coordinator = InheritedCoordinator
            ?? new ContextMenuCoordinator(this, ContextMenuCoordinator.DefaultShowChildContextMenuDelay);

        PopupRegistry.Add(this);
    }

    /// <inheritdoc/>
    public void Dispose()
        => Dispose(disposing: true);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCoreAsync().ConfigureAwait(false);

        Dispose(disposing: false);
    }

    /// <summary>
    /// Performs synchronous clean-up
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && !_isChildContextMenu)
            _coordinator?.Dispose();

        PopupRegistry.Remove(this);

        _disposed = true;
    }

    /// <summary>
    /// Performs asynchronous clean-up
    /// </summary>
    private async ValueTask DisposeAsyncCoreAsync()
    {
        // Marked before the first await, so an attach still loading sees it.
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        // Stops a show that is still waiting for its render or position.
        _requestVersion++;
        _pendingShow = null;

        // Blazor calls only DisposeAsync(), so Dispose(disposing: true) never gets to stop pending child context menus.
        if (!_isChildContextMenu)
            _coordinator?.Dispose();

        await RemoveJsAsync();

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;

        await _jsModule.DisposeAsync(Logger);
        _jsModule = null;
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (!_shouldRender)
            return false;

        _shouldRender = false;

        return true;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Taken over before the handler runs, so a show or close inside the handler is compared with this value
        // and reported after it.
        if (_renderedVisible != _reportedVisible)
        {
            _reportedVisible = _renderedVisible;

            await VisibilityChanged.InvokeAsync(_reportedVisible);
        }

        // The position depends on the size of the menu, so it is calculated only once the items matching the
        // requested filter are rendered.
        var pendingShow = _pendingShow;
        if (pendingShow is null || pendingShow.RequestVersion != _renderedRequestVersion)
            return;

        _pendingShow = null;

        await RevealAsync(pendingShow);
    }

    /// <inheritdoc />
    public Task ShowAsync(MouseEventArgs mouseEventArgs, ContextMenuItemFilter? itemFilter)
        => InvokeAsync(async () =>
        {
            var requestVersion = ++_requestVersion;
            _pendingShow = null;

            // Child context menus opened for the previous position or item filter would otherwise stay behind.
            if (_coordinator is not null)
                await _coordinator.ContextMenuShowingAsync(this);

            // Closed or shown again while its child context menus were closing.
            if (requestVersion != _requestVersion)
                return;

            _pendingShow = new PendingShow(requestVersion, mouseEventArgs);
            _itemFilter = itemFilter;

            // Renders the matching items while the menu is still hidden, see OnAfterRenderAsync().
            RequestRender();
        });

    /// <inheritdoc />
    public Task CloseAsync()
        => InvokeAsync(async () =>
        {
            var requestVersion = ++_requestVersion;
            _pendingShow = null;

            Closing?.Invoke();

            if (_coordinator is not null)
                await _coordinator.ContextMenuClosingAsync(this);

            // Shown again while its child context menus were closing.
            if (requestVersion != _requestVersion)
                return;

            if (!_visible || _disposedAsync)
                return;

            _visible = false;
            _position = null;
            _itemFilter = null;

            await EndObserveWindowPointerDownAsync();

            // Hides the menu, see OnAfterRenderAsync() for reporting it.
            RequestRender();
        });

    /// <summary>
    /// Called from the browser when the pointer is pressed, to tell whether it was pressed inside a visible context
    /// menu
    /// </summary>
    [JSInvokable]
    public VisibleContextMenus GetVisibleContextMenus()
    {
        var visibleContextMenus = _coordinator?.GetVisibleContextMenus() ?? [];

        return new VisibleContextMenus
        {
            HtmlElements = [.. visibleContextMenus.Select(c => c.ElementReference)],
            RequestVersion = _requestVersion,
        };
    }

    /// <summary>
    /// Called from the browser when the pointer was pressed outside of all context menus returned by
    /// <see cref="GetVisibleContextMenus"/>
    /// </summary>
    /// <remarks>
    /// Does nothing when the context menu has been shown or closed since, as by the right-click that pressed the
    /// pointer, which would otherwise be swallowed on a slow connection.
    /// </remarks>
    /// <param name="requestVersion"><see cref="VisibleContextMenus.RequestVersion"/> returned for the same press</param>
    [JSInvokable]
    public Task CloseOnPointerDownOutsideAsync(int requestVersion)
        => InvokeAsync(async () =>
        {
            if (requestVersion != _requestVersion)
                return;

            await CloseAsync();
        });

    private void RequestRender()
    {
        _shouldRender = true;
        StateHasChanged();
    }

    private async Task RevealAsync(PendingShow pendingShow)
    {
        // JS interop is needed only once the context menu is displayed for the first time.
        _attachJsTask ??= AttachJsAsync();
        await _attachJsTask;

        if (_jsInstance is null)
            return;

        var position = await _jsInstance.InvokeAsync<ContextMenuPosition>("calculatePosition",
            _elementReference, pendingShow.MouseEventArgs);

        if (pendingShow.RequestVersion != _requestVersion)
            return;

        _position = position;

        if (!_visible)
        {
            // Set before observing starts, so a close arriving meanwhile ends the observation again.
            _visible = true;

            await StartObserveWindowPointerDownAsync();

            // Closed while observing started, so the close has already reset the menu, which never appeared.
            if (pendingShow.RequestVersion != _requestVersion)
                return;
        }

        // Shows the menu, see OnAfterRenderAsync() for reporting it.
        RequestRender();
    }

    private async Task AttachJsAsync()
    {
        if (_jsInstance is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(ContextMenu).Assembly.GetName().Name}/context-menu/components/context-menu.js");

            // A menu removed while its script was loading has had its disposal run without seeing the module; a new
            // reference here would keep its window listener calling into it.
            if (_disposedAsync)
            {
                await _jsModule.DisposeAsync(Logger);
                _jsModule = null;
                return;
            }

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsInstance = await _jsModule.InvokeConstructorAsync("ContextMenu", Logger, _dotNetObjectReference);

            if (_disposedAsync)
                await DisposeJsInstanceAsync();
        }
    }

    private async Task RemoveJsAsync()
        => await DisposeJsInstanceAsync();

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsInstance.dispose() failed")]
    private static partial void InvokingJsInstanceDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsInstance failed")]
    private static partial void DisposingJsInstanceFailed(ILogger logger, Exception ex);

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

    private async Task StartObserveWindowPointerDownAsync()
    {
        if (_isChildContextMenu)
            return;

        if (_jsInstance is null)
            return;

        if (Interlocked.Exchange(ref _isObservingWindowPointerDown, 1) == 0)
            await _jsInstance.InvokeVoidAsync("startObserveWindowPointerDown");
    }

    private async Task EndObserveWindowPointerDownAsync()
    {
        if (_isChildContextMenu)
            return;

        if (_jsInstance is null)
            return;

        if (Interlocked.Exchange(ref _isObservingWindowPointerDown, 0) == 1)
            await _jsInstance.InvokeVoidAsync("endObserveWindowPointerDown");
    }

    /// <summary>
    /// Show request waiting for the render of its items before the context menu is positioned and revealed
    /// </summary>
    private sealed record PendingShow(int RequestVersion, MouseEventArgs MouseEventArgs);
}
