using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

/// <summary>
/// Drag-reordering of column headers: which column is being dragged, which gap it currently hovers, and what
/// the drop does to the column order.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <remarks>
/// A column can only be dropped among the columns sharing its pin side — dragging a left-pinned column into
/// the unpinned run would put it somewhere its pin placement cannot describe.
/// </remarks>
internal sealed class ColumnReorderInteraction<TItem>(ColumnLayout<TItem> columnLayout)
    where TItem : class
{
    private IAdvancedTableColumn<TItem>? _draggedColumn;

    // The gap the pointer is over, as the header reported it. Kept unnormalized so that leaving it can be
    // matched against the same pair that entered it; normalization happens when the indicator is drawn.
    private ColumnGap<TItem>? _activeGapZone;

    /// <summary>
    /// Whether a column is currently being dragged, which is what makes the headers render their gap zones.
    /// </summary>
    public bool IsDragging => _draggedColumn is not null;

    /// <summary>
    /// Records that <paramref name="column"/> is now being dragged.
    /// </summary>
    public void DragStart(IAdvancedTableColumn<TItem> column)
        => _draggedColumn = column;

    /// <summary>
    /// Marks the gap beside <paramref name="column"/> as the one the pointer is over.
    /// </summary>
    public void DragEnter(IAdvancedTableColumn<TItem> column, bool after)
    {
        if (_draggedColumn is null || _draggedColumn.PinSide != column.PinSide)
            return;

        _activeGapZone = new ColumnGap<TItem>(column, after);
    }

    /// <summary>
    /// Clears the marked gap, but only if it is still the one being left: the enter of the next gap can arrive
    /// before the leave of this one.
    /// </summary>
    public void DragLeave(IAdvancedTableColumn<TItem> column, bool after)
    {
        if (_activeGapZone is not { } zone || zone.Column != column || zone.After != after)
            return;

        _activeGapZone = null;
    }

    /// <summary>
    /// Moves the dragged column into the gap beside <paramref name="column"/> and ends the drag.
    /// </summary>
    public void Drop(IAdvancedTableColumn<TItem> column, bool after)
    {
        if (_draggedColumn is null || _draggedColumn.PinSide != column.PinSide)
            return;

        var gap = NormalizeGap(column, after);

        // Dropping a column onto its own gap is a move to where it already is.
        if (gap.Column != _draggedColumn)
            columnLayout.Move(_draggedColumn, gap.Column, gap.After);

        DragEnd();
    }

    /// <summary>
    /// Ends the drag without moving anything.
    /// </summary>
    public void DragEnd()
    {
        _activeGapZone = null;
        _draggedColumn = null;
    }

    /// <summary>
    /// The class marking <paramref name="column"/> as the edge the drop indicator is drawn on, or
    /// <see langword="null"/> when the active gap is elsewhere.
    /// </summary>
    public string? GetGapIndicatorClass(IAdvancedTableColumn<TItem> column)
    {
        if (_activeGapZone is not { } zone)
            return null;

        var gap = NormalizeGap(zone.Column, zone.After);

        if (gap.Column != column)
            return null;

        if (gap.After)
            return "gap-indicator-end";

        return "gap-indicator-start";
    }

    // The gap behind a column is the same place as the gap in front of the next one, so it is named by the
    // following column wherever there is one. That leaves exactly one name per gap, which is what lets the
    // indicator and the drop agree about which gap was meant.
    private ColumnGap<TItem> NormalizeGap(IAdvancedTableColumn<TItem> column, bool after)
    {
        if (!after)
            return new ColumnGap<TItem>(column, After: false);

        if (columnLayout.GetNextRenderedColumn(column) is { } nextColumn && nextColumn.PinSide == column.PinSide)
            return new ColumnGap<TItem>(nextColumn, After: false);

        return new ColumnGap<TItem>(column, After: true);
    }
}
