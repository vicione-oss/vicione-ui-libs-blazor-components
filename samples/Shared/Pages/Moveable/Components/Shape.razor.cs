using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;
using ViciOne.Ui.Blazor.Components.Moveable.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace Shared.Pages.Moveable.Components;

public sealed partial class Shape : IMoveableShape, IMoveable, IMoveHandle
{
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private bool _snapToGrid;
    private bool _snapToGridChanged;
    private Task? _moveInteractionAttachTask;
    private IMoveHandle? _moveHandle;
    private bool _moveHandleChanged;

    [CascadingParameter] public IMoveContainer Parent { get; set; } = default!;

    [Parameter] public bool Moveable { get; set; }
    [Parameter] public double? X { get; set; }
    [Parameter] public EventCallback<double?> XChanged { get; set; }
    [Parameter] public double? Y { get; set; }
    [Parameter] public EventCallback<double?> YChanged { get; set; }
    [Parameter] public bool SnapToGrid { get; set; }
    [Parameter] public bool WithMoveHandle { get; set; }

    [Inject] private IMoveInteraction MoveInteraction { get; set; } = default!;
    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SnapToGrid != _snapToGrid)
        {
            _snapToGridChanged = _moveInteractionAttachTask is not null;

            _snapToGrid = SnapToGrid;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Moveable)
        {
            if (_moveHandleChanged)
            {
                if (_moveInteractionAttachTask is not null)
                    await RemoveMoveInteractionAsync();
            }

            if (_moveInteractionAttachTask is null)
            {
                IPointerCaptureBehavior[]? pointerCaptureBehaviors = _snapToGrid ? [SnapToGridPointerCaptureBehavior] : null;

                _moveInteractionAttachTask = MoveInteraction.AttachAsync(this, pointerCaptureBehaviors);

                await _moveInteractionAttachTask;
            }
        }
        else
        {
            if (_moveInteractionAttachTask is not null)
                await RemoveMoveInteractionAsync();
        }

        if (Interlocked.CompareExchange(ref _snapToGridChanged, false, true))
        {
            if (_moveInteractionAttachTask is not null)
            {
                if (_snapToGrid)
                    await MoveInteraction.AddPointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
                else
                    await MoveInteraction.RemovePointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await RemoveMoveInteractionAsync().ConfigureAwait(false);
    }

    private async Task RemoveMoveInteractionAsync()
    {
        if (_moveInteractionAttachTask?.IsCompletedSuccessfully == true)
        {
            await MoveInteraction.RemoveAsync(this);

            _moveInteractionAttachTask = null;
        }
    }

    public ElementReference GetElementReference() => _elementReference;

    public IMoveContainer GetMoveContainer() => Parent;

    public IMoveHandle GetMoveHandle() => _moveHandle ?? this;

    public async Task UpdatePositionAsync(double x, double y)
    {
        X = x;

        if (XChanged.HasDelegate)
            await XChanged.InvokeAsync(x);

        Y = y;

        if (YChanged.HasDelegate)
            await YChanged.InvokeAsync(Y);

        await InvokeAsync(StateHasChanged);
    }

    void IMoveableShape.RegisterMoveHandle(IMoveHandle moveHandle)
    {
        if (moveHandle != _moveHandle)
        {
            _moveHandle = moveHandle;
            _moveHandleChanged = true;

            InvokeAsync(StateHasChanged);
        }
    }

    void IMoveableShape.UnregisterMoveHandle(IMoveHandle moveHandle)
    {
        if (moveHandle == _moveHandle)
        {
            _moveHandle = null;
            _moveHandleChanged = true;

            InvokeAsync(StateHasChanged);
        }
    }
}
