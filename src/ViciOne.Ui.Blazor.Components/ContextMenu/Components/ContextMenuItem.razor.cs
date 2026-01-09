using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
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
    private IJSObjectReference? _jsAttachResult;
    private Task? _attachJsTask;

    private bool _disposedAsync;

    private ElementReference? _htmlElementReference;

    private MouseLeaveDirection _childContextMenuMouseLeaveDirection;

    private readonly System.Timers.Timer _showChildContextMenuTimer = new() { AutoReset = false, Interval = 250 };
    private MouseEventArgs? _childContextMenuMouseEventArgs;
    private int _observingMouseLeave;

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

        ParentContextMenu.Hiding += ParentContextMenuHidingAsync;

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

        ParentContextMenu.Hiding -= ParentContextMenuHidingAsync;

        if (_showChildContextMenuTimer is not null)
        {
            _showChildContextMenuTimer.Stop();
            _showChildContextMenuTimer.Elapsed -= ShowChildContentMenuTimerElapsedAsync;
            _showChildContextMenuTimer.Dispose();
        }
    }

    private async Task AttachJsAsync()
    {
        if (_jsAttachResult is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(ContextMenu).Assembly.GetName().Name}/context-menu/components/context-menu-item.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", _dotNetObjectReference);
        }
    }

    private async Task RemoveJsAsync()
        => await DisposeJsAttachResultAsync();

    [LoggerMessage(Level = LogLevel.Error, Message = $"Invoking jsAttachResult.dispose() failed")]
    private static partial void InvokingJsAttachResultDisposeFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = $"Disposing jsAttachResult failed")]
    private static partial void DisposingJsAttachResultFailed(ILogger logger, Exception ex);

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

    private async Task MenuItemClickAsync(MouseEventArgs e)
    {
        if (!Enabled)
            return;

        await OnClick.InvokeAsync();

        if (_childContextMenu is null)
        {
            await RootContextMenu.HideAsync();

            return;
        }

        if (!_childContextMenu.Visible)
        {
            _childContextMenuMouseEventArgs = e;
            _showChildContextMenuTimer.Start();

            await StartObserveMouseLeaveAsync();
        }
    }

    private async Task ChildContentContextMenuVisibilityChangedAsync(bool isVisible)
    {
        _showChildContextMenuTimer.Stop();

        if (!isVisible)
            await EndObserveMouseLeaveAsync();
    }

    private async Task StartObserveMouseLeaveAsync()
    {
        if (Interlocked.Exchange(ref _observingMouseLeave, 1) == 0)
        {
            if (_jsAttachResult is null || _htmlElementReference is null)
                return;

            await _jsAttachResult.InvokeVoidAsync("startObserveMouseLeave", _htmlElementReference.Value);
        }
    }

    private async Task EndObserveMouseLeaveAsync()
    {
        if (Interlocked.Exchange(ref _observingMouseLeave, 0) == 1)
        {
            if (_jsAttachResult is null)
                return;

            await _jsAttachResult.InvokeVoidAsync("endObserveMouseLeave");
        }
    }

    private async Task MenuItemMouseEnterAsync(MouseEventArgs e)
    {
        if (!Enabled)
            return;

        if (_childContextMenu?.Visible == false)
        {
            _childContextMenuMouseEventArgs = e;
            _showChildContextMenuTimer.Start();

            await StartObserveMouseLeaveAsync();
        }
    }

    /// <summary>
    /// Called from the browser to notify the component about a mouse leave
    /// </summary>
    [JSInvokable]
    public async Task MouseLeaveAsync(MouseLeaveDirection direction)
    {
        _showChildContextMenuTimer.Stop();

        if (_childContextMenu?.Visible == true)
        {
            if (direction != _childContextMenuMouseLeaveDirection)
            {
                await EndObserveMouseLeaveAsync();

                await _childContextMenu.HideAsync();
            }
        }
        else
        {
            await EndObserveMouseLeaveAsync();
        }
    }

    private async void ParentContextMenuHidingAsync()
    {
        _showChildContextMenuTimer.Stop();
        await EndObserveMouseLeaveAsync();

        if (_childContextMenu?.Visible == true)
            await _childContextMenu.HideAsync();
    }

    private async void ShowChildContentMenuTimerElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        if (_jsAttachResult is null || _htmlElementReference is null || _childContextMenu is null || _childContextMenuMouseEventArgs is null)
            return;

        var childContextMenuPosition = await _jsAttachResult.InvokeAsync<ChildContextMenuPosition>("calculateChildContextMenuPosition",
            _htmlElementReference, _childContextMenu.ElementReference);

        if (childContextMenuPosition is null)
            return;

        _childContextMenuMouseEventArgs.PageX = childContextMenuPosition.X;
        _childContextMenuMouseEventArgs.PageY = childContextMenuPosition.Y;
        _childContextMenuMouseLeaveDirection = childContextMenuPosition.MouseLeaveDirection;

        await _childContextMenu.ShowAsync(_childContextMenuMouseEventArgs, ItemFilter);
    }

    IContextMenu? IContextMenuItem.GetChildContextMenu() => _childContextMenu;
}
