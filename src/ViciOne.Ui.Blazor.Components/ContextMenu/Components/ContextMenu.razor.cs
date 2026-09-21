using System.Collections.Concurrent;
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
    private ContextMenuItemFilter? _itemFilter;
    private readonly HashSet<IContextMenuItem> _items = [];
    private bool _isChildContextMenu;
    private int _isObservingWindowPointerDown;
    private bool _isRendering;
    private ContextMenuPosition? _position;
    private readonly System.Timers.Timer _renderTimer = new()
    {
        AutoReset = true,
        Enabled = false,
        Interval = 10
    };
    private bool _shouldRender = true;
    private readonly ConcurrentStack<ContextMenuState> _stateStack = new();
    private bool _visible;

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

    [CascadingParameter(Name = Constants.CascadingParameterNames.ParentContextMenu)]
    internal IContextMenu? ParentContextMenu { get; set; }

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
    [Parameter]
    public EventCallback<bool> VisibilityChanged { get; set; }

    [Inject]
    private IPopupRegistry PopupRegistry { get; set; } = default!;

    [Inject]
    private ILogger<ContextMenu> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    IContextMenu? IContextMenu.ParentContextMenu => ParentContextMenu;

    IEnumerable<IContextMenuItem> IContextMenu.Items => _items;

    /// <inheritdoc/>
    public event Action? Closing;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _renderTimer.Elapsed += OnRenderTimerElapsedAsync;
        _renderTimer.Start();

        PopupRegistry.Add(this);
    }

    private async void OnRenderTimerElapsedAsync(object? _, System.Timers.ElapsedEventArgs _2)
    {
        if (_isRendering)
            return;

        if (!_stateStack.TryPop(out var state))
            return;

        if (state.ShouldBeShown)
        {
            // when context menu should be displayed then we need JS interop
            if (_attachJsTask is null)
            {
                _attachJsTask = AttachJsAsync();
                await _attachJsTask;
            }

            if (_jsInstance is null || state.MouseEventArgs is null || _elementReference.Equals(default))
                return;

            _position = await _jsInstance.InvokeAsync<ContextMenuPosition>("calculatePosition",
                _elementReference, state.MouseEventArgs);

            _itemFilter = state.ItemFilter;

            if (!_visible)
            {
                _visible = true;

                await StartObserveWindowPointerDownAsync();

                await InvokeAsync(() => VisibilityChanged.InvokeAsync(true));
            }

            _shouldRender = true;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            if (_visible)
            {
                _visible = false;
                _position = null;
                _itemFilter = null;

                await EndObserveWindowPointerDownAsync();

                await InvokeAsync(() => VisibilityChanged.InvokeAsync(false));

                _shouldRender = true;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
        => _isChildContextMenu = ParentContextMenu is not null;

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

        if (disposing)
        {
            // dispose managed resources, nothing to do here yet 
        }

        PopupRegistry.Remove(this);

        _disposed = true;
    }

    /// <summary>
    /// Performs asynchronous clean-up
    /// </summary>
    private async ValueTask DisposeAsyncCoreAsync()
    {
        if (_disposedAsync)
            return;

        await RemoveJsAsync();

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;

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

        _renderTimer.Elapsed -= OnRenderTimerElapsedAsync;
        _renderTimer.Dispose();

        _disposedAsync = true;
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (_shouldRender)
        {
            _shouldRender = false;
            _isRendering = true;

            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        _isRendering = false;
    }

    /// <inheritdoc />
    public async Task ShowAsync(MouseEventArgs mouseEventArgs, ContextMenuItemFilter? itemFilter)
    {
        var newState = new ContextMenuState
        {
            ItemFilter = itemFilter,
            MouseEventArgs = mouseEventArgs,
            ShouldBeShown = true,
        };

        _stateStack.Push(newState);

        _itemFilter = itemFilter;

        _shouldRender = true;
        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    [JSInvokable]
    public Task CloseAsync()
    {
        Closing?.Invoke();

        var newState = new ContextMenuState();

        _stateStack.Push(newState);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called from the browser to determine which HTML element represents a visible context menu
    /// </summary>
    [JSInvokable]
    public ElementReference[] GetVisibleContextMenuHtmlElements()
    {
        var result = new List<ElementReference>();

        if (_visible)
            result.Add(_elementReference);

        var visibleChildContextMenus = GetVisibleChildContextMenus(_items);

        result.AddRange(visibleChildContextMenus.Select(c => c.ElementReference));

        return [.. result];
    }

    private static IEnumerable<IContextMenu> GetVisibleChildContextMenus(IEnumerable<IContextMenuItem> contextMenuItems)
    {
        foreach (var contextMenuItem in contextMenuItems)
        {
            var childContextMenu = contextMenuItem.GetChildContextMenu();
            if (childContextMenu?.Visible == true)
            {
                yield return childContextMenu;

                var visibleChildContextMenus = GetVisibleChildContextMenus(childContextMenu.Items);
                foreach (var visibleChildContextMenu in visibleChildContextMenus)
                    yield return visibleChildContextMenu;
            }
        }
    }

    private async Task AttachJsAsync()
    {
        if (_jsInstance is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(ContextMenu).Assembly.GetName().Name}/context-menu/components/context-menu.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsInstance = await _jsModule.InvokeConstructorAsync("ContextMenu", Logger, _dotNetObjectReference);
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

    void IContextMenu.RegisterContextMenuItem(IContextMenuItem contextMenuItem)
        => _items.Add(contextMenuItem);

    void IContextMenu.UnregisterContextMenuItem(IContextMenuItem contextMenuItem)
        => _items.Remove(contextMenuItem);
}
