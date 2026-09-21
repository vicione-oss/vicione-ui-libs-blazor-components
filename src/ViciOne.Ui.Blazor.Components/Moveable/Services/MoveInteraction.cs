using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Moveable.Components;
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
    private readonly Dictionary<IMoveable, IJSObjectReference> _jsInstances = [];
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<MoveInteraction>? _dotNetObjectReference;

    public string StartedCssClass => "moving";
    public string OngoingCssClass => "moving-ongoing";
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
                if (_jsInstances.ContainsKey(moveable))
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
                    OngoingCssClass = OngoingCssClass,
                    EndedCssClass = EndedCssClass,
                    DotNetObject = _dotNetObjectReference
                };

                if (pointerCaptureBehaviors is not null)
                {
                    var pointerCaptureBehaviorJsObjects = await Task.WhenAll(pointerCaptureBehaviors.Select(b => b.GetJsObjectAsync()));

                    moveInteractionContext.PointerCaptureBehaviors = [.. pointerCaptureBehaviorJsObjects.OfType<IJSObjectReference>()];
                }

                var jsInstance = await _jsModule.InvokeConstructorAsync("MoveInteraction", logger,
                    cancellationToken, moveInteractionContext);

                if (jsInstance is null)
                    return;

                _moveableIds.Add(moveable, moveableId);
                _moveables.Add(moveableId, moveable);

                _jsInstances.Add(moveable, jsInstance);
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

                if (_jsInstances.TryGetValue(moveable, out var jsInstance))
                {
                    await jsInstance.InvokeVoidAsync("dispose", logger, cancellationToken);
                    await jsInstance.DisposeAsync(logger);

                    _jsInstances.Remove(moveable);
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

                if (_jsInstances.TryGetValue(moveable, out var jsInstance))
                    await jsInstance.InvokeVoidAsync("addPointerCaptureBehavior", logger, cancellationToken, jsObject);
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

                if (_jsInstances.TryGetValue(moveable, out var jsInstance))
                    await jsInstance.InvokeVoidAsync("removePointerCaptureBehavior", logger, cancellationToken, jsObject);
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

            var jsDisposeTasks = _jsInstances.Values.Select(jsInstance => jsInstance.InvokeVoidAsync("dispose", logger));
            await Task.WhenAll(jsDisposeTasks);

            foreach (var jsInstance in _jsInstances.Values)
                await jsInstance.DisposeAsync(logger);

            _jsInstances.Clear();

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
    public async Task MoveablePointerUpAsync(Guid moveableId, double x, double y)
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
