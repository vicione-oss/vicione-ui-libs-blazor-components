using Microsoft.AspNetCore.Components;
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

    IContextMenu IContextMenuItem.ParentContextMenu => ParentContextMenu;

    IContextMenu? IContextMenuItem.ChildContextMenu => _childContextMenu;

    ContextMenuItemFilter? IContextMenuItem.ItemFilter => ItemFilter;

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

        await Coordinator.ItemRemovedAsync(this);

        await EndObserveMouseLeaveAsync();
        await RemoveJsAsync();

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;

        await _jsModule.DisposeAsync(Logger);
        _jsModule = null;
    }

    /// <summary>
    /// Called from the browser to notify the component about a mouse leave
    /// </summary>
    [JSInvokable]
    public Task MouseLeaveAsync(MouseLeaveDirection direction)
        => Coordinator.MouseLeftItemAsync(this, direction);

    async Task<ChildContextMenuPosition?> IContextMenuItem.CalculateChildContextMenuPositionAsync()
    {
        var jsInstance = await GetJsInstanceAsync();

        if (jsInstance is null || _htmlElementReference is null || _childContextMenu is null)
            return null;

        return await jsInstance.InvokeAsync<ChildContextMenuPosition?>("calculateChildContextMenuPosition",
            _htmlElementReference, _childContextMenu.ElementReference);
    }

    Task IContextMenuItem.StartObserveMouseLeaveAsync()
        => StartObserveMouseLeaveAsync();

    Task IContextMenuItem.EndObserveMouseLeaveAsync()
        => EndObserveMouseLeaveAsync();

    private async Task MenuItemClickAsync()
    {
        if (!Enabled)
            return;

        await OnClick.InvokeAsync();

        if (_childContextMenu is null)
        {
            await Coordinator.RootContextMenu.CloseAsync();

            return;
        }

        await Coordinator.ExpandAsync(this);
    }

    private async Task MenuItemMouseEnterAsync()
    {
        if (!Enabled)
            return;

        await Coordinator.ExpandAsync(this);
    }

    private async Task AttachJsAsync()
    {
        if (_jsInstance is null)
        {
            _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import",
                $"./_content/{typeof(ContextMenu).Assembly.GetName().Name}/context-menu/components/context-menu-item.js");

            // An item removed while its script was loading has had its disposal run without seeing the module.
            if (_disposedAsync)
            {
                await _jsModule.DisposeAsync(Logger);
                _jsModule = null;
                return;
            }

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            _jsInstance = await _jsModule.InvokeConstructorAsync("ContextMenuItem", Logger, _dotNetObjectReference);

            if (_disposedAsync)
                await DisposeJsInstanceAsync();
        }
    }

    /// <summary>
    /// Waits for the script that is attached after the first render, as the item can be hovered before it is ready
    /// </summary>
    private async Task<IJSObjectReference?> GetJsInstanceAsync()
    {
        if (_attachJsTask is not null)
            await _attachJsTask;

        return _jsInstance;
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

    private async Task StartObserveMouseLeaveAsync()
    {
        var jsInstance = await GetJsInstanceAsync();

        if (jsInstance is null || _htmlElementReference is null)
            return;

        if (Interlocked.Exchange(ref _observingMouseLeave, 1) == 0)
            await jsInstance.InvokeVoidAsync("startObserveMouseLeave", _htmlElementReference.Value);
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
}
