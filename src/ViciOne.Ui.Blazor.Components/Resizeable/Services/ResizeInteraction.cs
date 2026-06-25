using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Models;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Services;

internal sealed class ResizeInteraction(ILogger<ResizeInteraction> logger, IJSRuntime jsRuntime)
    : IResizeInteraction, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposedAsync;

    private string? _assemblyName;
    private readonly Dictionary<IResizeable, Guid> _resizeableIds = [];
    private readonly Dictionary<Guid, IResizeable> _resizeables = [];
    private readonly Dictionary<IResizeable, IJSObjectReference> _jsAttachResults = [];
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ResizeInteraction>? _dotNetObjectReference;

    public string StartedCssClass => "resizing";
    public string OngoingCssClass => "resizing-ongoing";
    public string EndedCssClass => "resized";

    public async Task AttachAsync(IResizeable resizeable, IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_jsAttachResults.ContainsKey(resizeable))
                    return;

                _assemblyName ??= typeof(ResizeInteraction).Assembly.GetName().Name;

                _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken,
                    $"./_content/{_assemblyName}/resizeable/resize-interaction.js");

                _dotNetObjectReference ??= DotNetObjectReference.Create(this);

                var resizeableId = Guid.NewGuid();

                var resizeInteractionContext = new ResizeInteractionContext
                {
                    ResizeableId = resizeableId,
                    Resizeable = resizeable.GetElementReference(),
                    ResizeHandles = [.. resizeable.GetResizeHandles().Select(resizeHandle => new ResizeHandleInfo
                    {
                        Element = resizeHandle.GetElementReference(),
                        Position = resizeHandle.Position
                    })],
                    ResizeContainer = resizeable.GetResizeContainer().GetElementReference(),
                    MinimumWidth = resizeable.GetMinimumWidth(),
                    MinimumHeight = resizeable.GetMinimumHeight(),
                    StartedCssClass = StartedCssClass,
                    OngoingCssClass = OngoingCssClass,
                    EndedCssClass = EndedCssClass,
                    DotNetObject = _dotNetObjectReference
                };

                if (pointerCaptureBehaviors is not null)
                {
                    var pointerCaptureBehaviorJsObjects = await Task.WhenAll(pointerCaptureBehaviors.Select(b => b.GetJsObjectAsync()));

                    resizeInteractionContext.PointerCaptureBehaviors = [.. pointerCaptureBehaviorJsObjects.OfType<IJSObjectReference>()];
                }

                var jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", cancellationToken, resizeInteractionContext);

                _resizeableIds.Add(resizeable, resizeableId);
                _resizeables.Add(resizeableId, resizeable);

                _jsAttachResults.Add(resizeable, jsAttachResult);
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

    public async Task RemoveAsync(IResizeable resizeable)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_resizeableIds.TryGetValue(resizeable, out var resizeableId))
                {
                    _resizeables.Remove(resizeableId);
                    _resizeableIds.Remove(resizeable);
                }

                if (_jsAttachResults.TryGetValue(resizeable, out var jsAttachResult))
                {
                    await jsAttachResult.InvokeVoidAsync("dispose", logger, cancellationToken);
                    await jsAttachResult.DisposeAsync(logger);

                    _jsAttachResults.Remove(resizeable);
                }
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

    public async Task AddPointerCaptureBehaviorAsync(IResizeable resizeable, IPointerCaptureBehavior pointerCaptureBehavior)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (await pointerCaptureBehavior.GetJsObjectAsync() is not IJSObjectReference jsObject)
                    return;

                if (_jsAttachResults.TryGetValue(resizeable, out var jsAttachResult))
                    await jsAttachResult.InvokeVoidAsync("addPointerCaptureBehavior", logger, cancellationToken, jsObject);
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

    public async Task RemovePointerCaptureBehaviorAsync(IResizeable resizeable, IPointerCaptureBehavior pointerCaptureBehavior)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (await pointerCaptureBehavior.GetJsObjectAsync() is not IJSObjectReference jsObject)
                    return;

                if (_jsAttachResults.TryGetValue(resizeable, out var jsAttachResult))
                    await jsAttachResult.InvokeVoidAsync("removePointerCaptureBehavior", logger, cancellationToken, jsObject);
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

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _semaphore.WaitAsync();
        try
        {
            _resizeables.Clear();
            _resizeableIds.Clear();

            var jsDisposeTasks = _jsAttachResults.Values.Select(jsAttachResult => jsAttachResult.InvokeVoidAsync("dispose", logger));
            await Task.WhenAll(jsDisposeTasks);

            foreach (var jsAttachResult in _jsAttachResults.Values)
                await jsAttachResult.DisposeAsync(logger);

            _jsAttachResults.Clear();

            _dotNetObjectReference?.Dispose();

            await _jsModule.DisposeAsync(logger);
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }

    [JSInvokable]
    public async Task OnUpdatePositionAndSizeAsync(Guid resizeableId, DomRect domRect)
    {
        try
        {
            await _semaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                if (_resizeables.TryGetValue(resizeableId, out var resizeable))
                    await resizeable.UpdatePositionAndSizeAsync(domRect);
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
}
