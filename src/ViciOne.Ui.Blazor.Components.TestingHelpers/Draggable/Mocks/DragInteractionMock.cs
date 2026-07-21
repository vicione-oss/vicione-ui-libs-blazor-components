using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Mocks;

internal sealed class DragInteractionMock : IDragInteraction
{
    public bool IsDragStartEventHandlerAssigned => DragStart is not null;

    public string StartedCssClass { get; } = string.Empty;
    public string OngoingCssClass { get; } = string.Empty;
    public string EndedCssClass { get; } = string.Empty;

    public event EventHandler<DragStartEventArgs>? DragStart;

    public Task AttachAsync(IDraggable draggable, ModifierKey? modifierKey = null,
        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null,
        IDragGhost? dragGhost = null)
            => Task.CompletedTask;

    public Task RemoveAsync(IDraggable draggable)
        => Task.CompletedTask;

    public Task AddPointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => Task.CompletedTask;

    public Task RemovePointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior)
        => Task.CompletedTask;

    public void StartDrag(IDraggable draggable, out IList<IDropzone> dropzones)
    {
        if (DragStart is not null)
        {
            var dragStartEventArgs = new DragStartEventArgs { Draggable = draggable };
            DragStart.Invoke(this, dragStartEventArgs);
            dropzones = dragStartEventArgs.Dropzones;
        }
        else
        {
            dropzones = [];
        }
    }
}
