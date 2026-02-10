using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;
using ViciOne.Ui.Blazor.Components.Moveable.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.Moveable.Services;

internal sealed class MoveInteraction(ILogger<MoveInteraction> logger, IJSRuntime jsRuntime)
    : IMoveInteraction, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposedAsync;

    private string? _assemblyName;
    private readonly Dictionary<IMoveable, Guid> _moveableIds = [];
    private readonly Dictionary<Guid, IMoveable> _moveables = [];
    private readonly Dictionary<IMoveable, IJSObjectReference> _jsAttachResults = [];
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MoveInteraction>? _dotNetObjectReference;

    public string StartedCssClass => "moving";
    public string EndedCssClass => "moved";

    public async Task AttachAsync(IMoveable moveable, IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_jsAttachResults.ContainsKey(moveable))
                    return;

                _assemblyName ??= typeof(MoveInteraction).Assembly.GetName().Name;

                _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken,
                    $"./_content/{_assemblyName}/moveable/move-interaction.js");

                _dotNetObjectReference ??= DotNetObjectReference.Create(this);

                var moveableId = Guid.NewGuid();

                var moveInteractionContext = new MoveInteractionContext
                {
                    MoveableId = moveableId,
                    Moveable = moveable.GetElementReference(),
                    MoveHandle = moveable.GetMoveHandle().GetElementReference(),
                    MoveContainer = moveable.GetMoveContainer().GetElementReference(),
                    StartedCssClass = StartedCssClass,
                    EndedCssClass = EndedCssClass,
                    DotNetObject = _dotNetObjectReference
                };

                if (pointerCaptureBehaviors is not null)
                {
                    var pointerCaptureBehaviorJsObjects = await Task.WhenAll(pointerCaptureBehaviors.Select(b => b.GetJsObjectAsync()));

                    moveInteractionContext.PointerCaptureBehaviors = [.. pointerCaptureBehaviorJsObjects.OfType<IJSObjectReference>()];
                }

                var jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", cancellationToken, moveInteractionContext);

                _moveableIds.Add(moveable, moveableId);
                _moveables.Add(moveableId, moveable);

                _jsAttachResults.Add(moveable, jsAttachResult);
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

    public async Task RemoveAsync(IMoveable moveable)
    {
        if (_disposedAsync)
            return;

        try
        {
            var cancellationToken = _cancellationTokenSource.Token;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_moveableIds.TryGetValue(moveable, out var moveableId))
                {
                    _moveables.Remove(moveableId);
                    _moveableIds.Remove(moveable);
                }

                if (_jsAttachResults.TryGetValue(moveable, out var jsAttachResult))
                {
                    await jsAttachResult.InvokeVoidAsync("dispose", logger, cancellationToken);
                    await jsAttachResult.DisposeAsync(logger);

                    _jsAttachResults.Remove(moveable);
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

    public async Task AddPointerCaptureBehaviorAsync(IMoveable moveable, IPointerCaptureBehavior pointerCaptureBehavior)
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

                if (_jsAttachResults.TryGetValue(moveable, out var jsAttachResult))
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

    public async Task RemovePointerCaptureBehaviorAsync(IMoveable moveable, IPointerCaptureBehavior pointerCaptureBehavior)
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

                if (_jsAttachResults.TryGetValue(moveable, out var jsAttachResult))
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
            _moveables.Clear();
            _moveableIds.Clear();

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
    public async Task OnMoveablePointerUpAsync(Guid moveableId, double x, double y)
    {
        try
        {
            await _semaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                if (_moveables.TryGetValue(moveableId, out var moveable))
                    await moveable.UpdatePositionAsync(x, y);
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
