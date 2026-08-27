using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace Shared.Pages.Draggable.Components;

public sealed partial class Shape : ComponentBase, IDraggable, IHasLabel, IAsyncDisposable
{
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private ModifierKey? _modifierKey;
    private IDragGhost? _dragGhost;
    private bool _snapToGrid;
    private bool _snapToGridChanged;
    private bool _modifierKeyChanged;
    private bool _dragGhostChanged;
    private Task? _dragInteractionAttachTask;

    [Parameter, EditorRequired] public string Label { get; set; } = default!;
    [Parameter] public bool Draggable { get; set; }
    [Parameter] public ModifierKey? ModifierKey { get; set; }
    [Parameter] public bool SnapToGrid { get; set; }
    [Parameter] public IDragGhost? DragGhost { get; set; }

    [Inject] private IDragInteraction DragInteraction { get; set; } = default!;
    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;

    public ElementReference GetElementReference() => _elementReference;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SnapToGrid != _snapToGrid)
        {
            _snapToGrid = SnapToGrid;

            _snapToGridChanged = _dragInteractionAttachTask is not null;
        }

        if (ModifierKey != _modifierKey)
        {
            _modifierKey = ModifierKey;

            _modifierKeyChanged = _dragInteractionAttachTask is not null;
        }

        if (DragGhost != _dragGhost)
        {
            _dragGhost = DragGhost;

            _dragGhostChanged = _dragInteractionAttachTask is not null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Draggable)
        {
            var modifierKeyChanged = Interlocked.CompareExchange(ref _modifierKeyChanged, false, true);
            var dragGhostChanged = Interlocked.CompareExchange(ref _dragGhostChanged, false, true);

            if ((modifierKeyChanged || dragGhostChanged) && _dragInteractionAttachTask is not null)
                await RemoveDragInteractionAsync();

            if (_dragInteractionAttachTask is null)
            {
                IPointerCaptureBehavior[]? pointerCaptureBehaviors = _snapToGrid ? [SnapToGridPointerCaptureBehavior] : null;

                _dragInteractionAttachTask = DragInteraction.AttachAsync(this, ModifierKey, pointerCaptureBehaviors, DragGhost);

                await _dragInteractionAttachTask;
            }
        }
        else
        {
            if (_dragInteractionAttachTask is not null)
                await RemoveDragInteractionAsync();
        }

        if (Interlocked.CompareExchange(ref _snapToGridChanged, false, true))
        {
            if (_dragInteractionAttachTask is not null)
            {
                if (_snapToGrid)
                    await DragInteraction.AddPointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
                else
                    await DragInteraction.RemovePointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
            }
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
