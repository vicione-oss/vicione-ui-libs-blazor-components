using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Services;

internal sealed class HtmlElementHelper(IJSRuntime jsRuntime, ILogger<HtmlElementHelper> logger)
    : IHtmlElementHelper, IAsyncDisposable
{
    private IJSObjectReference? _jsModule;

    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposed;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private async ValueTask<IJSObjectReference?> GetJsModuleAsync()
    {
        if (_disposed)
            return null;

        try
        {
            await _semaphore.WaitAsync(_cancellationTokenSource.Token);

            try
            {
                _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import",
                    _cancellationTokenSource.Token,
                    $"./_content/{GetType().Assembly.GetName().Name}/breadcrumb/html-element-helper.js");

                return _jsModule;
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

    public async Task<IEnumerable<DomRect>> GetBoundingClientRectsAsync(IEnumerable<ElementReference> htmlElements)
    {
        var jsModule = await GetJsModuleAsync();
        if (jsModule == null)
            return [];

        try
        {
            return await jsModule.InvokeAsync<IEnumerable<DomRect>>(
                "getBoundingClientRects",
                _cancellationTokenSource.Token,
                htmlElements
            );
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully

            return [];
        }
        catch (ObjectDisposedException)
        {
            // jsModule or CancellationTokenSource already disposed, nothing we can do, return gracefully

            return [];
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
            await _jsModule.DisposeAsync(logger);
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}
