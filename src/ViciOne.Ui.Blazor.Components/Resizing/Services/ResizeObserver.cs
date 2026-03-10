using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;

namespace ViciOne.Ui.Blazor.Components.Resizing.Services;

internal sealed class ResizeObserver : IResizeObserver, IAsyncDisposable
{
    private readonly ILogger<ResizeObserver> _logger;

    private readonly Dictionary<string, ElementReference> _elementReferences = [];

    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ResizeObserver>? _objRef;
    private IJSObjectReference? _module;
    private IJSObjectReference? _jsInstance;

    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposed;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event Action<ElementSizeChangedEventArgs>? ElementSizeChanged;
    public event Func<ElementSizeChangedEventArgs, Task>? ElementSizeChangedAsync;

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
    public void SizeChanged(string elementId, DomRect domRect, CssStyleDeclaration? style)
    {
        if (!_elementReferences.TryGetValue(elementId, out var elementReference))
            return;

        var args = new ElementSizeChangedEventArgs { ElementReference = elementReference, DomRect = domRect, Style = style };
        ElementSizeChanged?.Invoke(args);
        ElementSizeChangedAsync?.Invoke(args);
    }

    public async Task ObserveAsync(ElementReference elementReference, bool includeStyle = false)
    {
        if (elementReference.Id == null)
            return;

        var instance = await GetInstanceAsync();
        if (instance == null)
            return;

        _elementReferences.Add(elementReference.Id, elementReference);

        try
        {
            await instance.InvokeVoidAsync("observe", _cancellationTokenSource.Token, elementReference, elementReference.Id,
                _objRef, includeStyle);
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

    public async Task UnobserveAsync(ElementReference elementReference)
    {
        var instance = await GetInstanceAsync();
        if (instance == null)
            return;

        // will break if we have several observers on same element
        _elementReferences.Remove(elementReference.Id);

        try
        {
            await instance.InvokeVoidAsync("unobserve", _cancellationTokenSource.Token, elementReference);
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // CancellationTokenSource already disposed, nothing we can do, return gracefully
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
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
