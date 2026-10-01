using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;

/// <summary>
/// Internal table row component implementing <see cref="IDraggable"/> and <see cref="IDraggableRowSet{TItem}"/>
/// for the Draggable feature.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public sealed partial class AdvancedTableRow<TItem> : ComponentBase, IDraggable, IDraggableRowSet<TItem>, IAsyncDisposable
    where TItem : class
{
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private Task? _attachTask;
    private string? _attachedElementReferenceId;
    private bool _attachFaulted;

    // The drag payload, resolved once at drag start and never cleared (the drag infrastructure raises no
    // drag-end signal to the row). Overwritten at the next drag start; read via IDraggableRowSet.Items.
    private IReadOnlyList<TItem>? _frozenPayload;

    [Inject]
    private IDragInteraction DragInteraction { get; set; } = default!;

    [Inject]
    private ITableRowDragGhost TableRowDragGhost { get; set; } = default!;

    [Inject]
    private ILogger<AdvancedTableRow<TItem>> Logger { get; set; } = default!;

    /// <summary>The data item this row represents.</summary>
    [Parameter, EditorRequired]
    public required TItem Item { get; set; }

    /// <summary>When <see langword="true"/>, this row participates in drag interactions.</summary>
    [Parameter] public bool Draggable { get; set; }

    /// <summary>The cell content rendered inside the row.</summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Table-owned <see cref="ITableDragPayloadResolver{TItem}"/>.
    /// </summary>
    [Parameter]
    public ITableDragPayloadResolver<TItem>? DragPayloadResolver { get; set; }

    /// <summary>CSS class string applied to the row element.</summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>Click handler forwarded to the row element.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>Pointer-enter handler forwarded to the row element.</summary>
    [Parameter]
    public EventCallback<PointerEventArgs> OnPointerEnter { get; set; }

    /// <summary>Double-click handler forwarded to the row element.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }

    /// <summary>Context-menu (right-click) handler forwarded to the row element.</summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnContextMenu { get; set; }

    /// <summary>When <see langword="true"/>, the row suppresses the browser's native context menu.</summary>
    [Parameter]
    public bool ContextMenuPreventDefault { get; set; }

    /// <inheritdoc/>
    bool IDraggable.Draggable => Draggable;

    /// <inheritdoc/>
    IReadOnlyList<TItem> IDraggableRowSet<TItem>.Items => _frozenPayload ?? [Item];

    /// <inheritdoc/>
    ElementReference IDraggable.GetElementReference() => _elementReference;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Draggable)
        {
            // The JavaScript side resolves the element reference by its capture id when the interop call runs,
            // so a row element replaced while an attach is in flight leaves the row silently unwired.
            if (_attachTask is null || _attachedElementReferenceId != _elementReference.Id)
            {
                if (_attachTask is not null)
                    await RemoveInteractionAsync();

                await AttachInteractionAsync();
            }
        }
        else if (_attachTask is not null)
        {
            await RemoveInteractionAsync();
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposedAsync)
            return;

        _disposedAsync = true;

        await RemoveInteractionAsync();
    }

    /// <inheritdoc/>
    // Awaited by the drag interaction before it raises DragStart, so a drop policy deciding on this drag
    // already reads its payload rather than the previous drag's or the at-rest [Item].
    Task IDraggable.PrepareDragStartAsync()
        => Draggable ? InvokeAsync(DragStartCoreAsync) : Task.CompletedTask;

    // One InvokeAsync continuation so no render slips between the selection mutation and the freeze:
    // resolve payload (mutation + its SelectedItemsChanged) → freeze the snapshot the drop side reads
    // via IDraggableRowSet.Items.
    private async Task DragStartCoreAsync()
    {
        IReadOnlyList<TItem> payload = [Item];

        if (DragPayloadResolver is not null)
            payload = await DragPayloadResolver.ResolvePayloadAsync(Item);

        _frozenPayload = payload;
    }

    private async Task AttachInteractionAsync()
    {
        _attachedElementReferenceId = _elementReference.Id;

        try
        {
            // Table rows use the table-row drag ghost so a dragged <tr> keeps its rendered width
            // instead of collapsing to its content.
            _attachTask = DragInteraction.AttachAsync(this, dragGhost: TableRowDragGhost);
            await _attachTask;

            _attachFaulted = false;
        }
        catch (ObjectDisposedException)
        {
            // Disposed mid-attach; teardown is handled by RemoveInteractionAsync.
        }
        catch (Exception exception)
        {
            // A row element replaced while the attach is in flight faults it, and OnAfterRenderAsync
            // re-attaches. Only a fault with no attach succeeding in between left the row un-draggable.
            _attachTask = null;

            if (_attachFaulted)
                FailedToAttachToDragInteraction(Logger, exception, nameof(AdvancedTableRow<>));
            else
                RetryingAttachToDragInteraction(Logger, exception, nameof(AdvancedTableRow<>));

            _attachFaulted = true;
        }
    }

    private async Task RemoveInteractionAsync()
    {
        // Await the in-flight attach so the remove runs after registration completes; swallow its
        // failure since RemoveAsync is a keyed no-op and must run unconditionally to clean up.
        try
        {
            if (_attachTask is not null)
                await _attachTask;
        }
        catch
        {
            // Intentionally ignored — see above.
        }

        _attachTask = null;
        _attachedElementReferenceId = null;
        _attachFaulted = false;

        try
        {
            await DragInteraction.RemoveAsync(this);
        }
        catch (ObjectDisposedException)
        {
            // The drag interaction was already torn down; nothing left to remove.
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to attach {Component} to the drag interaction.")]
    private static partial void FailedToAttachToDragInteraction(ILogger logger, Exception ex, string component);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Attaching {Component} to the drag interaction faulted; retrying on the next render.")]
    private static partial void RetryingAttachToDragInteraction(ILogger logger, Exception ex, string component);
}
