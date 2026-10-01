using Microsoft.AspNetCore.Components;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.Shared.Rows.DragAndDrop.Components;

public sealed partial class RowDropzone : ComponentBase, IDropzone, IAsyncDisposable
{
    private ElementReference _elementReference;
    private bool _highlighted;
    private bool _dragEntered;
    private ExampleTableItem? _lastDroppedItem;
    private int _lastDroppedCount;

    [Inject] private IDragInteraction DragInteraction { get; set; } = default!;

    public ElementReference GetElementReference() => _elementReference;

    protected override void OnInitialized()
        => DragInteraction.DragStart += HandleDragStart;

    public ValueTask DisposeAsync()
    {
        DragInteraction.DragStart -= HandleDragStart;
        return ValueTask.CompletedTask;
    }

    private void HandleDragStart(object? sender, DragStartEventArgs args)
    {
        if (args.Draggable is not IDraggableRowSet<ExampleTableItem>)
            return;

        args.Dropzones.Add(this);
        _highlighted = true;
        InvokeAsync(StateHasChanged);
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

        if (draggable is IDraggableRowSet<ExampleTableItem> set)
        {
            _lastDroppedItem = set.Items.Count > 0 ? set.Items[0] : null;
            _lastDroppedCount = set.Items.Count;
        }

        await InvokeAsync(StateHasChanged);
    }
}
