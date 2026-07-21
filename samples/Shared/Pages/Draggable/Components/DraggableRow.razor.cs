using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace Shared.Pages.Draggable.Components;

public sealed partial class DraggableRow : ComponentBase, IDraggable, IHasLabel, IAsyncDisposable
{
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private Task? _dragInteractionAttachTask;

    [Parameter, EditorRequired]
    public string Name { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Status { get; set; } = default!;

    [Parameter]
    public bool Draggable { get; set; }

    [Parameter]
    public IDragGhost? DragGhost { get; set; }

    [Inject]
    private IDragInteraction DragInteraction { get; set; } = default!;

    public string Label => Name;

    public ElementReference GetElementReference() => _elementReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Draggable)
        {
            if (_dragInteractionAttachTask is null)
            {
                _dragInteractionAttachTask = DragInteraction.AttachAsync(this, dragGhost: DragGhost);

                await _dragInteractionAttachTask;
            }
        }
        else
        {
            await RemoveDragInteractionAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await RemoveDragInteractionAsync();
    }

    private async Task RemoveDragInteractionAsync()
    {
        if (_dragInteractionAttachTask?.IsCompletedSuccessfully == true)
        {
            await DragInteraction.RemoveAsync(this);

            _dragInteractionAttachTask = null;
        }
    }
}
