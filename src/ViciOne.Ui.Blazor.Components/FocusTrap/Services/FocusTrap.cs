using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.FocusTrap.Components;

namespace ViciOne.Ui.Blazor.Components.FocusTrap.Services;

internal sealed class FocusTrap(ILogger<FocusTrap> logger, IJSRuntime jsRuntime) : IFocusTrap, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposedAsync;

    private string? _assemblyName;
    private readonly Dictionary<IFocusTrappable, IJSObjectReference> _jsInstances = [];
    private readonly Dictionary<IFocusTrappable, string> _elementIds = [];
    private IJSObjectReference? _jsModule;

    public async Task AttachAsync(IFocusTrappable focusTrappable)
    {
        if (_disposedAsync)
            return;

        var element = focusTrappable.GetElementReference();

        if (string.IsNullOrEmpty(element.Id))
            return; // element is not rendered yet, hence nothing can be trapped yet

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_elementIds.TryGetValue(focusTrappable, out var elementId) && elementId == element.Id)
                    return;

                if (!_jsInstances.TryGetValue(focusTrappable, out var jsInstance))
                {
                    _assemblyName ??= typeof(FocusTrap).Assembly.GetName().Name;

                    _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken,
                        $"./_content/{_assemblyName}/focus-trap/focus-trap.js");

                    jsInstance = await _jsModule.InvokeConstructorAsync("FocusTrap", logger, cancellationToken);

                    if (jsInstance is null)
                        return;

                    _jsInstances.Add(focusTrappable, jsInstance);
                }

                await jsInstance.InvokeVoidAsync("attach", logger, cancellationToken, element);

                _elementIds[focusTrappable] = element.Id;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
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

    public async Task RemoveAsync(IFocusTrappable focusTrappable)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _elementIds.Remove(focusTrappable);

                if (_jsInstances.TryGetValue(focusTrappable, out var jsInstance))
                {
                    await jsInstance.InvokeVoidAsync("dispose", logger, cancellationToken);
                    await jsInstance.DisposeAsync(logger);

                    _jsInstances.Remove(focusTrappable);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (JSDisconnectedException)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability#javascript-interop-calls-without-a-circuit
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

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _semaphore.WaitAsync();
        try
        {
            _elementIds.Clear();

            var jsDisposeTasks = _jsInstances.Values.Select(jsInstance => jsInstance.InvokeVoidAsync("dispose", logger));
            await Task.WhenAll(jsDisposeTasks);

            foreach (var jsInstance in _jsInstances.Values)
                await jsInstance.DisposeAsync(logger);

            _jsInstances.Clear();

            await _jsModule.DisposeAsync(logger);
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}
