using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Models;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

internal sealed class DragInteraction(ILogger<DragInteraction> logger, IJSRuntime jsRuntime)
    : IDragInteraction, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AsyncLocal<bool> _semaphoreAcquired = new();
    private bool _disposedAsync;

    private readonly Dictionary<IDraggable, Guid> _draggableIds = [];
    private readonly Dictionary<Guid, IDraggable> _draggables = [];
    private readonly Dictionary<IDraggable, IJSObjectReference> _jsAttachResults = [];
    private readonly Dictionary<Guid, IDropzone> _dropzones = [];

    private string? _assemblyName;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<DragInteraction>? _dotNetObjectReference;

    public string StartedCssClass => "dragging";
    public string OngoingCssClass => "dragging-ongoing";
    public string EndedCssClass => "dragged";

    public event EventHandler<DragStartEventArgs>? DragStart;

    public Task AttachAsync(IDraggable draggable, ModifierKey? modifierKey = null,
        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null,
        IDragGhost? dragGhost = null)
        => ExecuteGuardedAsync(async () =>
        {
            if (_jsAttachResults.ContainsKey(draggable))
                return;

            _assemblyName ??= typeof(DragInteraction).Assembly.GetName().Name;

            _jsModule ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", _cancellationTokenSource.Token,
                $"./_content/{_assemblyName}/draggable/drag-interaction.js");

            _dotNetObjectReference ??= DotNetObjectReference.Create(this);

            var draggableId = Guid.NewGuid();

            var dragInteractionContext = new DragInteractionContext
            {
                DraggableId = draggableId,
                Draggable = draggable.GetElementReference(),
                StartedCssClass = StartedCssClass,
                OngoingCssClass = OngoingCssClass,
                EndedCssClass = EndedCssClass,
                ModifierKey = modifierKey,
                DotNetObject = _dotNetObjectReference,
                DragGhostJsModule = dragGhost?.GetJsModule()
            };

            if (pointerCaptureBehaviors is not null)
            {
                var pointerCaptureBehaviorJsObjects = await Task.WhenAll(pointerCaptureBehaviors.Select(b => b.GetJsObjectAsync()));

                dragInteractionContext.PointerCaptureBehaviors = [.. pointerCaptureBehaviorJsObjects.OfType<IJSObjectReference>()];
            }

            var jsAttachResult = await _jsModule.InvokeAsync<IJSObjectReference>("attach", _cancellationTokenSource.Token,
                dragInteractionContext);

            _draggableIds.Add(draggable, draggableId);
            _draggables.Add(draggableId, draggable);

            _jsAttachResults.Add(draggable, jsAttachResult);
        });

    public Task RemoveAsync(IDraggable draggable)
        => ExecuteGuardedAsync(async () =>
        {
            if (_draggableIds.TryGetValue(draggable, out var draggableId))
            {
                _draggables.Remove(draggableId);
                _draggableIds.Remove(draggable);
            }

            if (_jsAttachResults.TryGetValue(draggable, out var jsAttachResult))
            {
                await jsAttachResult.InvokeVoidAsync("dispose", logger, _cancellationTokenSource.Token);
                await jsAttachResult.DisposeAsync(logger);

                _jsAttachResults.Remove(draggable);
            }
        });

    public Task AddPointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => ExecuteGuardedAsync(async () =>
        {
            if (await pointerCaptureBehavior.GetJsObjectAsync() is not IJSObjectReference jsObject)
                return;

            if (_jsAttachResults.TryGetValue(draggable, out var jsAttachResult))
                await jsAttachResult.InvokeVoidAsync("addPointerCaptureBehavior", logger, _cancellationTokenSource.Token, jsObject);
        });

    public Task RemovePointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => ExecuteGuardedAsync(async () =>
        {
            if (await pointerCaptureBehavior.GetJsObjectAsync() is not IJSObjectReference jsObject)
                return;

            if (_jsAttachResults.TryGetValue(draggable, out var jsAttachResult))
                await jsAttachResult.InvokeVoidAsync("removePointerCaptureBehavior", logger, _cancellationTokenSource.Token, jsObject);
        });

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        var semaphoreAcquired = false;
        if (!_semaphoreAcquired.Value)
        {
            await _semaphore.WaitAsync();
            semaphoreAcquired = true;
        }

        try
        {
            _draggables.Clear();
            _draggableIds.Clear();
            _dropzones.Clear();

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
            if (semaphoreAcquired)
                _semaphore.Release();

            _semaphore.Dispose();
        }
    }

    [JSInvokable]
    public Task<IEnumerable<DropzoneDescriptor>> DragStartAsync(Guid draggableId)
        => ExecuteGuardedAsync<IEnumerable<DropzoneDescriptor>>(async () =>
        {
            _dropzones.Clear();

            var dragStartEvent = DragStart;
            if (dragStartEvent is null)
                return [];

            if (!_draggables.TryGetValue(draggableId, out var draggable))
                return [];

            var args = new DragStartEventArgs { Draggable = draggable };
            dragStartEvent.Invoke(this, args);

            // Reverse so last-rendered dropzones come first, reflecting visual z-order for bounds checking on the JavaScript side.
            var dropzones = args.Dropzones.Distinct().Reverse();
            var descriptors = new List<DropzoneDescriptor>();

            foreach (var dropzone in dropzones)
            {
                var dropzoneId = Guid.NewGuid();

                _dropzones.Add(dropzoneId, dropzone);

                descriptors.Add(new DropzoneDescriptor { Id = dropzoneId, Element = dropzone.GetElementReference() });
            }

            return descriptors;
        }, []);

    [JSInvokable]
    public Task DragEnterAsync(Guid draggableId, Guid dropzoneId)
        => ExecuteGuardedAsync(async () =>
        {
            if (_draggables.TryGetValue(draggableId, out var draggable) &&
                _dropzones.TryGetValue(dropzoneId, out var dropzone))
            {
                await dropzone.DragEnterAsync(draggable);
            }
        });

    [JSInvokable]
    public Task DragLeaveAsync(Guid dropzoneId)
        => ExecuteGuardedAsync(async () =>
        {
            if (_dropzones.TryGetValue(dropzoneId, out var dropzone))
                await dropzone.DragLeaveAsync();
        });

    [JSInvokable]
    public Task DragEndAsync(Guid draggableId, double x, double y)
        => ExecuteGuardedAsync(async () =>
        {
            if (!_draggables.TryGetValue(draggableId, out var draggable))
                return;

            await Task.WhenAll(_dropzones.Values.Select(dz => dz.DragEndAsync(draggable, x, y)));
        });

    [JSInvokable]
    public Task DragDroppedAsync(Guid draggableId, Guid dropzoneId, double x, double y)
        => ExecuteGuardedAsync(async () =>
        {
            if (_draggables.TryGetValue(draggableId, out var draggable) &&
                _dropzones.TryGetValue(dropzoneId, out var dropzone))
            {
                await dropzone.DragDroppedAsync(draggable, x, y);
            }
        });

    private async Task ExecuteGuardedAsync(Func<Task> action)
    {
        if (_disposedAsync)
            return;

        try
        {
            if (_semaphoreAcquired.Value)
            {
                await action();

                return;
            }

            await _semaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                _semaphoreAcquired.Value = true;

                await action();
            }
            finally
            {
                _semaphoreAcquired.Value = false;

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

    private async Task<T> ExecuteGuardedAsync<T>(Func<Task<T>> action, T defaultValue = default!)
    {
        if (_disposedAsync)
            return defaultValue;

        try
        {
            if (_semaphoreAcquired.Value)
                return await action();

            await _semaphore.WaitAsync(_cancellationTokenSource.Token);
            try
            {
                _semaphoreAcquired.Value = true;

                return await action();
            }
            finally
            {
                _semaphoreAcquired.Value = false;

                _semaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully

            return defaultValue;
        }
        catch (ObjectDisposedException)
        {
            // Semaphore or CancellationTokenSource already disposed, nothing we can do, return gracefully

            return defaultValue;
        }
    }
}
