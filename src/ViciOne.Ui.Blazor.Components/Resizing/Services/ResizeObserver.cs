using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;

namespace ViciOne.Ui.Blazor.Components.Resizing.Services;

internal sealed partial class ResizeObserver : IResizeObserver, IAsyncDisposable
{
    private readonly ILogger<ResizeObserver> _logger;

    private readonly ConcurrentDictionary<string, ElementReference> _elementReferences = [];

    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ResizeObserver>? _dotNetObjectReference;
    private IJSObjectReference? _module;
    private IJSObjectReference? _jsInstance;

    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposed;
    private bool _jsDisconnectedExceptionOccurred;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event Action<ElementSizeChangedEventArgs>? ElementSizeChanged;
    public event Func<ElementSizeChangedEventArgs, Task>? ElementSizeChangedAsync;

    public ResizeObserver(IJSRuntime jsRuntime, ILogger<ResizeObserver> logger)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    private async ValueTask<IJSObjectReference?> GetJsInstanceAsync(CancellationToken cancellationToken)
    {
        if (_jsInstance is not null)
            return _jsInstance;

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import",
            cancellationToken,
            $"./_content/{GetType().Assembly.GetName().Name}/resizing/resize-observer.js");

        _jsInstance = await _module.InvokeAsync<IJSObjectReference>("createInstance", cancellationToken);

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
        if (_disposed)
            return;

        if (elementReference.Id is null)
            return;

        if (_jsDisconnectedExceptionOccurred)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (!_elementReferences.TryAdd(elementReference.Id, elementReference))
                    return; // Element is already observed, no need to observe again

                var jsInstance = await GetJsInstanceAsync(cancellationToken);
                if (jsInstance is null)
                    return;

                await jsInstance.InvokeVoidAsync("observe", cancellationToken,
                    [elementReference, elementReference.Id, _dotNetObjectReference, includeStyle]);
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
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit

            _jsDisconnectedExceptionOccurred = true;
        }
        catch (Exception ex)
        {
            InvokingVoidFailed(_logger, ex, "observe", [elementReference, elementReference.Id, _dotNetObjectReference, includeStyle]);
        }
    }

    public async Task UnobserveAsync(ElementReference elementReference)
    {
        if (_disposed)
            return;

        if (elementReference.Id is null)
            return;

        if (_jsDisconnectedExceptionOccurred)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (_jsInstance is null)
                    return; // ObserveAsync() was most likely not called before

                if (!_elementReferences.TryRemove(elementReference.Id, out _))
                    return; // Element was not observed before or already removed

                await _jsInstance.InvokeVoidAsync("unobserve", cancellationToken, elementReference);
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
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit

            _jsDisconnectedExceptionOccurred = true;
        }
        catch (Exception ex)
        {
            InvokingVoidFailed(_logger, ex, "unobserve", [elementReference]);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        if (_jsInstance is not null)
        {
            await _semaphore.WaitAsync();

            try
            {
                try
                {
                    foreach (var elementReference in _elementReferences.Values)
                        await _jsInstance.InvokeVoidAsync("unobserve", elementReference);
                }
                catch (JSDisconnectedException)
                {
                    // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
                }

                await _jsInstance.DisposeAsync(_logger);
                _jsInstance = null;

                await _module.DisposeAsync(_logger);
                _module = null;

                _dotNetObjectReference?.Dispose();
                _dotNetObjectReference = null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        _semaphore.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Invoking jsObjectReference.{identifier}({args}) failed")]
    private static partial void InvokingVoidFailed(ILogger logger, Exception ex, string identifier, object?[]? args);
}
