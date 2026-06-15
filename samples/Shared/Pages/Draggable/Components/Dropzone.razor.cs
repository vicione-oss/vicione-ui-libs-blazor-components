using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace Shared.Pages.Draggable.Components;

public sealed partial class Dropzone : ComponentBase, IHasLabel, IDropzone, IAsyncDisposable
{
    private ElementReference _elementReference;
    private bool _highlighted;
    private bool _dragEntered;
    private int? _previousGridSize;

    [Parameter, EditorRequired] public string Label { get; set; } = default!;
    [Parameter] public int? GridSize { get; set; }

    [Inject] private IDragInteraction DragInteraction { get; set; } = default!;
    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;
    [Inject] private IDropPolicy<IDropzone> DropPolicy { get; set; } = default!;
    [Inject] private IDropHandler<IDropzone> DropHandler { get; set; } = default!;

    public ElementReference GetElementReference() => _elementReference;

    protected override async Task OnParametersSetAsync()
    {
        if (GridSize != _previousGridSize)
        {
            _previousGridSize = GridSize;

            if (GridSize.HasValue)
                await SnapToGridPointerCaptureBehavior.SetGridSizeAsync(GridSize.Value);
        }
    }

    protected override void OnInitialized()
        => DragInteraction.DragStart += DragStart;

    public ValueTask DisposeAsync()
    {
        DragInteraction.DragStart -= DragStart;

        return ValueTask.CompletedTask;
    }

    private void DragStart(object? sender, DragStartEventArgs args)
    {
        if (DropPolicy.Accepts(args.Draggable, this))
        {
            args.Dropzones.Add(this);

            _highlighted = true;

            InvokeAsync(StateHasChanged);
        }
    }

    public Task DragEnterAsync(IDraggable draggable)
    {
        _dragEntered = true;

        return InvokeAsync(StateHasChanged);
    }

    public Task DragLeaveAsync()
    {
        _dragEntered = false;

        return InvokeAsync(StateHasChanged);
    }

    public Task DragEndAsync(IDraggable draggable, double x, double y)
    {
        _highlighted = false;
        _dragEntered = false;

        return InvokeAsync(StateHasChanged);
    }

    public async Task DragDroppedAsync(IDraggable draggable, double x, double y)
    {
        _highlighted = false;
        _dragEntered = false;

        await DropHandler.DragDroppedAsync(draggable, x, y, this);

        await InvokeAsync(StateHasChanged);
    }
}
