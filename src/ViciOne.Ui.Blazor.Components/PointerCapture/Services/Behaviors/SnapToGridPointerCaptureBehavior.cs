using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.PointerCapture.Services;

internal sealed class SnapToGridPointerCaptureBehavior(ILogger<SnapToGridPointerCaptureBehavior> logger, IJSRuntime jsRuntime)
    : ISnapToGridPointerCaptureBehavior, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private IJSObjectReference? _jsObject;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<SnapToGridPointerCaptureBehavior>? _dotNetObjectReference;
    private int? _gridSize;
    private bool _disposedAsync;

    public async Task SetGridSizeAsync(int value)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (value == _gridSize)
                    return;

                _gridSize = value;

                if (_jsObject is not null)
                    await _jsObject.InvokeVoidAsync("setGridSize", logger, cancellationToken, value);
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

    public async Task<IJSObjectReference?> GetJsObjectAsync()
    {
        if (_disposedAsync)
            return null;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_jsObject is null)
                {
                    _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken,
                        $"./_content/{typeof(SnapToGridPointerCaptureBehavior).Assembly.GetName().Name}/pointer-capture/snap-to-grid-pointer-capture-behavior.js");

                    _dotNetObjectReference ??= DotNetObjectReference.Create(this);

                    _jsObject = await _jsModule.InvokeAsync<IJSObjectReference>("createInstance", cancellationToken, _gridSize);
                }

                return _jsObject;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully

            return null;
        }
        catch (ObjectDisposedException)
        {
            // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully

            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _semaphore.WaitAsync();
        try
        {
            await _jsObject.DisposeAsync(logger);

            _dotNetObjectReference?.Dispose();

            await _jsModule.DisposeAsync(logger);
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}
