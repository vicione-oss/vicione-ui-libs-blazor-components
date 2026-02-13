using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Resizing.Services;

internal sealed class ResizeObserver : IResizeObserver, IAsyncDisposable
{
    private readonly ILogger<ResizeObserver> _logger;

    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ResizeObserver>? _objRef;
    private IJSObjectReference? _module;
    private IJSObjectReference? _jsInstance;

    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposed;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event Action<Guid, DomRect>? ElementSizeChanged;
    public event Func<Guid, DomRect, Task>? ElementSizeChangedAsync;

    public ResizeObserver(IJSRuntime jsRuntime, ILogger<ResizeObserver> logger)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        _objRef = DotNetObjectReference.Create(this);
    }

    private async ValueTask<IJSObjectReference?> GetInstanceAsync()
    {
        if (_disposed)
            return null;

        if (_jsInstance == null)
        {
            try
            {
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);

                try
                {
                    _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import",
                        _cancellationTokenSource.Token,
                        $"./_content/{GetType().Assembly.GetName().Name}/resizing/resize-observer.js");

                    _jsInstance = await _module.InvokeAsync<IJSObjectReference>("createInstance", _cancellationTokenSource.Token);
                }
                finally
                {
                    _semaphore.Release();

                }
            }
            catch (OperationCanceledException)
            {
                // Nothing to do here, return gracefully
            }
            catch (ObjectDisposedException)
            {
                // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully
            }
        }

        return _jsInstance;
    }

    [JSInvokable]
    public void SizeChanged(string elementId, DomRect elementRect)
    {
        var elementGuid = new Guid(elementId);

        ElementSizeChanged?.Invoke(elementGuid, elementRect);
        ElementSizeChangedAsync?.Invoke(elementGuid, elementRect);
    }

    public async Task ObserveAsync(ElementReference elementRef)
    {
        var instance = await GetInstanceAsync();
        if (instance == null)
            return;

        try
        {
            await instance.InvokeVoidAsync("observe", _cancellationTokenSource.Token, elementRef, _objRef);
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
    }

    public async Task UnobserveAsync(ElementReference elementRef)
    {
        var instance = await GetInstanceAsync();
        if (instance == null)
            return;

        try
        {
            await instance.InvokeVoidAsync("unobserve", _cancellationTokenSource.Token, elementRef);
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _semaphore.WaitAsync();

        try
        {
            await _jsInstance.DisposeAsync(_logger);
            _jsInstance = null;

            await _module.DisposeAsync(_logger);
            _module = null;

            _objRef?.Dispose();
            _objRef = null;
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}
