using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace Shared.Pages.Draggable.Components;

internal sealed class SampleDropHandler : IDropHandler<IDropzone>
{
    public string? LastDroppedLabel { get; private set; }
    public string? LastTargetLabel { get; private set; }

    public event Action? Changed;

    public Task DragDroppedAsync(IDraggable draggable, double x, double y, IDropzone target)
    {
        LastDroppedLabel = (draggable as IHasLabel)?.Label ?? target.GetType().Name;
        LastTargetLabel = (target as IHasLabel)?.Label ?? draggable.GetType().Name;

        Changed?.Invoke();

        return Task.CompletedTask;
    }
}
