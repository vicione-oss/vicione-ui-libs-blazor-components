using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;

internal sealed class TestDragInteraction : IDragInteraction
{
    private readonly List<IDraggable> _attached = [];

    // Continuations run asynchronously so an awaiting test never resumes inline on the renderer's dispatcher,
    // which is the thread that signals the removal.
    private readonly TaskCompletionSource _removed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string StartedCssClass => "dragging";
    public string OngoingCssClass => "ongoing-dragging";
    public string EndedCssClass => "dragged";
    public IDraggable? LastAttached { get; private set; }
    public IDragGhost? LastDragGhost { get; private set; }
    public IDraggable? LastRemoved { get; private set; }
    public int AttachCount { get; private set; }
    public int RemoveCount { get; private set; }

    /// <summary>Every draggable currently attached, in attach order — lets a test drag a specific row.</summary>
    public IReadOnlyList<IDraggable> Attached => _attached;

    /// <summary>
    /// Completes once <see cref="RemoveAsync"/> has run for the first time. Awaiting it is the only reliable way
    /// to observe a removal: it changes no component state, so it triggers no render for a render-driven
    /// <c>WaitForAssertion</c> to re-check on.
    /// </summary>
    public Task Removed => _removed.Task;

    /// <summary>When set, <see cref="AttachAsync"/> awaits this gate, letting a test hold an attach in flight.</summary>
    public TaskCompletionSource? AttachGate { get; set; }

    /// <summary>When <see langword="true"/>, the next <see cref="AttachAsync"/> faults to simulate a failed attach.</summary>
    public bool FaultNextAttach { get; set; }

    public event EventHandler<DragStartEventArgs>? DragStart;

    public async Task AttachAsync(IDraggable draggable, ModifierKey? modifierKey = null,
        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null,
        IDragGhost? dragGhost = null)
    {
        AttachCount++;

        if (FaultNextAttach)
        {
            FaultNextAttach = false;
            throw new InvalidOperationException("Simulated attach failure.");
        }

        if (AttachGate is not null)
            await AttachGate.Task;

        LastAttached = draggable;
        LastDragGhost = dragGhost;
        _attached.Add(draggable);
    }

    public Task RemoveAsync(IDraggable draggable)
    {
        RemoveCount++;
        LastRemoved = draggable;
        _attached.Remove(draggable);
        _removed.TrySetResult();

        return Task.CompletedTask;
    }

    public Task AddPointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => Task.CompletedTask;

    public Task RemovePointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => Task.CompletedTask;

    /// <summary>Starts a drag the way <c>DragInteraction</c> does: the draggable is prepared before any listener runs.</summary>
    public async Task RaiseDragStartAsync(IDraggable draggable)
    {
        await draggable.PrepareDragStartAsync();

        DragStart?.Invoke(this, new DragStartEventArgs { Draggable = draggable });
    }
}
