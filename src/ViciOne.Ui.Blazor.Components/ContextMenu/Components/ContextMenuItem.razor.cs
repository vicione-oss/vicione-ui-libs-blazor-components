using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Component for rendering a context menu item
/// </summary>
public sealed partial class ContextMenuItem : ContextMenuItemBase, IContextMenuItem, IHasIcon, IAsyncDisposable
{
    private IContextMenu? _childContextMenu;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ContextMenuItem>? _dotNetObjectReference;
    private IJSObjectReference? _jsInstance;
    private Task? _attachJsTask;

    private bool _disposedAsync;

    private ElementReference? _htmlElementReference;

    private MouseLeaveDirection _childContextMenuMouseLeaveDirection;

    private readonly System.Timers.Timer _showChildContextMenuTimer = new() { AutoReset = false, Interval = 250 };
    private MouseEventArgs? _childContextMenuMouseEventArgs;
    private int _observingMouseLeave;

    private volatile bool _childContextMenuShowPending;

    /// <summary>
    /// Content of the child context menu displayed when the item is clicked / focused / hovered
    /// </summary>
    [Parameter] public RenderFragment<ContextMenuItem>? ChildContent { get; set; }

    /// <summary>
    /// True when the item should be enabled, otherwise false
    /// </summary>
    [Parameter] public bool Enabled { get; set; } = true;

    /// <summary>
    /// Raised when the item has been clicked
    /// </summary>
    [Parameter] public EventCallback OnClick { get; set; }

    /// <summary>
    /// Text displayed in the item
    /// </summary>
    [Parameter] public string Text { get; set; } = string.Empty;

    /// <inheritdoc/>
    [Parameter] public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    [Parameter] public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    [Parameter] public string? IconData { get; set; }

    /// <summary>
    /// True when a separator for visual grouping should be rendered, otherwise false
    /// </summary>
    [Parameter] public bool BeginGroup { get; set; }

    [Inject] private ILogger<ContextMenu> Logger { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        ParentContextMenu.RegisterContextMenuItem(this);

        ParentContextMenu.Closing += ParentContextMenuClosingAsync;

        _showChildContextMenuTimer.Elapsed += ShowChildContentMenuTimerElapsedAsync;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (ChildContent is not null && _attachJsTask is null)
        {
            _attachJsTask = AttachJsAsync();

            await _attachJsTask;
        }
    }

    /// <inheritdoc/>

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await EndObserveMouseLeaveAsync();
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

        ParentContextMenu.UnregisterContextMenuItem(this);

        ParentContextMenu.Closing -= ParentContextMenuClosingAsync;

        if (_showChildContextMenuTimer is not null)
        {
            CancelShowChildContextMenuDelayed();

            _showChildContextMenuTimer.Elapsed -= ShowChildContentMenuTimerElapsedAsync;
            _showChildContextMenuTimer.Dispose();
        }
    }

    private async Task AttachJsAsync()
    {
        if (_jsInstance is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(ContextMenu).Assembly.GetName().Name}/context-menu/components/context-menu-item.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsInstance = await _jsModule.InvokeConstructorAsync("ContextMenuItem", Logger, _dotNetObjectReference);
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

    private async Task MenuItemClickAsync(MouseEventArgs e)
    {
        if (!Enabled)
            return;

        await OnClick.InvokeAsync();

        if (_childContextMenu is null)
        {
            await RootContextMenu.CloseAsync();

            return;
        }

        if (!_childContextMenu.Visible && !_childContextMenuShowPending)
        {
            await CloseOtherChildContextMenusAsync();

            ShowChildContextMenuDelayed(e);

            await StartObserveMouseLeaveAsync();
        }
    }

    private async Task ChildContentContextMenuVisibilityChangedAsync(bool isVisible)
    {
        CancelShowChildContextMenuDelayed();

        if (!isVisible)
            await EndObserveMouseLeaveAsync();
    }

    private async Task StartObserveMouseLeaveAsync()
    {
        if (Interlocked.Exchange(ref _observingMouseLeave, 1) == 0)
        {
            if (_jsInstance is null || _htmlElementReference is null)
                return;

            await _jsInstance.InvokeVoidAsync("startObserveMouseLeave", _htmlElementReference.Value);
        }
    }

    private async Task EndObserveMouseLeaveAsync()
    {
        if (Interlocked.Exchange(ref _observingMouseLeave, 0) == 1)
        {
            if (_jsInstance is null)
                return;

            await _jsInstance.InvokeVoidAsync("endObserveMouseLeave");
        }
    }

    private async Task MenuItemMouseEnterAsync(MouseEventArgs e)
    {
        if (!Enabled)
            return;

        if (_childContextMenu?.Visible == false)
        {
            await CloseOtherChildContextMenusAsync();

            ShowChildContextMenuDelayed(e);

            await StartObserveMouseLeaveAsync();
        }
    }

    /// <summary>
    /// Called from the browser to notify the component about a mouse leave
    /// </summary>
    [JSInvokable]
    public async Task MouseLeaveAsync(MouseLeaveDirection direction)
    {
        CancelShowChildContextMenuDelayed();

        if (_childContextMenu?.Visible == true)
        {
            if (direction != _childContextMenuMouseLeaveDirection)
            {
                await EndObserveMouseLeaveAsync();

                await _childContextMenu.CloseAsync();
            }
        }
        else
        {
            await EndObserveMouseLeaveAsync();
        }
    }

    private async void ParentContextMenuClosingAsync()
    {
        CancelShowChildContextMenuDelayed();

        await EndObserveMouseLeaveAsync();

        if (_childContextMenu?.Visible == true)
            await _childContextMenu.CloseAsync();
    }

    private async void ShowChildContentMenuTimerElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        if (_jsInstance is null ||
            _htmlElementReference is null ||
            _childContextMenu is null ||
            _childContextMenuMouseEventArgs is null)
        {
            return;
        }

        var childContextMenuPosition = await _jsInstance.InvokeAsync<ChildContextMenuPosition>("calculateChildContextMenuPosition",
            _htmlElementReference, _childContextMenu.ElementReference);

        if (childContextMenuPosition is null)
            return;

        _childContextMenuMouseEventArgs.PageX = childContextMenuPosition.X;
        _childContextMenuMouseEventArgs.PageY = childContextMenuPosition.Y;
        _childContextMenuMouseLeaveDirection = childContextMenuPosition.MouseLeaveDirection;

        _childContextMenuShowPending = true;

        await _childContextMenu.ShowAsync(_childContextMenuMouseEventArgs, ItemFilter);
    }

    /// <inheritdoc/>
    async Task IContextMenuItem.CloseChildContextMenuAsync()
    {
        CancelShowChildContextMenuDelayed();

        await EndObserveMouseLeaveAsync();

        if (_childContextMenu?.Visible == true)
            await _childContextMenu.CloseAsync();
    }

    private async Task CloseOtherChildContextMenusAsync()
    {
        // Close any sibling child context menus that are open or pending to be shown
        foreach (var sibling in ParentContextMenu.Items)
        {
            if (sibling != this)
                await sibling.CloseChildContextMenuAsync();
        }
    }

    private void ShowChildContextMenuDelayed(MouseEventArgs mouseEventArgs)
    {
        _childContextMenuMouseEventArgs = mouseEventArgs;
        _showChildContextMenuTimer.Start();
    }

    private void CancelShowChildContextMenuDelayed()
    {
        _showChildContextMenuTimer.Stop();
        _childContextMenuShowPending = false;
    }

    IContextMenu? IContextMenuItem.GetChildContextMenu() => _childContextMenu;
}
