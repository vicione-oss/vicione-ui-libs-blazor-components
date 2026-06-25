using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace Shared.Pages.Resizeable.Components;

public sealed partial class Shape : IResizeable, IAsyncDisposable
{
    private readonly List<IResizeHandle> _resizeHandles = [];
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private bool _resizeHandlesChanged;
    private bool _snapToGrid;
    private bool _snapToGridChanged;
    private Task? _resizeInteractionAttachTask;

    [CascadingParameter] public IResizeContainer Parent { get; set; } = default!;

    [Parameter] public bool Resizeable { get; set; }
    [Parameter] public bool SnapToGrid { get; set; }
    [Parameter, EditorRequired] public DomRect? DomRect { get; set; }
    [Parameter] public EventCallback<DomRect?> DomRectChanged { get; set; }

    [Inject] private IResizeInteraction ResizeInteraction { get; set; } = default!;
    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SnapToGrid != _snapToGrid)
        {
            _snapToGridChanged = _resizeInteractionAttachTask is not null;

            _snapToGrid = SnapToGrid;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Resizeable)
        {
            if (_resizeHandlesChanged)
            {
                if (_resizeInteractionAttachTask is not null)
                    await RemoveResizeInteractionAsync();

                _resizeHandlesChanged = false;
            }

            if (_resizeInteractionAttachTask is null && _resizeHandles.Count > 0)
            {
                IPointerCaptureBehavior[]? pointerCaptureBehaviors = _snapToGrid ? [SnapToGridPointerCaptureBehavior] : null;

                _resizeInteractionAttachTask = ResizeInteraction.AttachAsync(this, pointerCaptureBehaviors);

                await _resizeInteractionAttachTask;
            }
        }
        else
        {
            if (_resizeInteractionAttachTask is not null)
                await RemoveResizeInteractionAsync();
        }

        if (_snapToGridChanged)
        {
            _snapToGridChanged = false;

            if (_resizeInteractionAttachTask is not null)
            {
                if (_snapToGrid)
                    await ResizeInteraction.AddPointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
                else
                    await ResizeInteraction.RemovePointerCaptureBehaviorAsync(this, SnapToGridPointerCaptureBehavior);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposedAsync)
            return;

        _disposedAsync = true;

        await RemoveResizeInteractionAsync().ConfigureAwait(false);
    }

    private async Task RemoveResizeInteractionAsync()
    {
        if (_resizeInteractionAttachTask?.IsCompletedSuccessfully == true)
        {
            await ResizeInteraction.RemoveAsync(this);

            _resizeInteractionAttachTask = null;
        }
    }

    public ElementReference GetElementReference() => _elementReference;

    public IReadOnlyCollection<IResizeHandle> GetResizeHandles() => _resizeHandles;

    public IResizeContainer GetResizeContainer() => Parent;

    public double GetMinimumWidth() => 100;

    public double GetMinimumHeight() => 100;

    public async Task UpdatePositionAndSizeAsync(DomRect? domRect)
    {
        DomRect = domRect;

        if (DomRectChanged.HasDelegate)
            await DomRectChanged.InvokeAsync(DomRect);
    }

    public void RegisterResizeHandle(IResizeHandle resizeHandle)
    {
        if (!_resizeHandles.Contains(resizeHandle))
        {
            _resizeHandles.Add(resizeHandle);
            _resizeHandlesChanged = true;

            InvokeAsync(StateHasChanged);
        }
    }

    public void UnregisterResizeHandle(IResizeHandle resizeHandle)
    {
        if (_resizeHandles.Remove(resizeHandle))
        {
            _resizeHandlesChanged = true;

            InvokeAsync(StateHasChanged);
        }
    }
}
