using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Enums;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

/// <summary>
/// The columns a table has and the geometry it renders them with: which columns exist, the order they appear
/// in, how wide each one is and where the pinned ones sit.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <remarks>
/// Holds no opinion about filtering, sorting or selection — it answers "what does the table draw, and how
/// wide", and nothing else. The table keeps ownership of the column <em>state</em> objects, because those are
/// tied to the events a column raises rather than to its geometry.
/// </remarks>
internal sealed class ColumnLayout<TItem>
    where TItem : class
{
    private readonly List<IAdvancedTableColumn<TItem>> _columns = [];

    // Widths JavaScript measured or the user pinned by dragging a resize handle, keyed by column id. A column
    // with no entry falls back to the width its own parameter declares.
    private readonly Dictionary<string, ColumnWidthOverride> _widthOverrides = [];

    private List<IAdvancedTableColumn<TItem>> _orderedColumns = [];
    private List<RenderedColumn<TItem>> _renderedColumns = [];

    /// <summary>
    /// Every registered column, hidden ones included, in the order the table renders them — not the order they
    /// registered in: all left pins, then the unpinned columns, then all right pins.
    /// </summary>
    public IReadOnlyList<IAdvancedTableColumn<TItem>> Columns => _orderedColumns;

    /// <summary>
    /// The columns that actually render, in render order, each paired with the style that places it when pinned.
    /// </summary>
    public IReadOnlyList<RenderedColumn<TItem>> RenderedColumns => _renderedColumns;

    /// <summary>
    /// Whether the pinned columns have moved since JavaScript last placed them. Set by every change that can
    /// shift them; the table clears it once it has asked for a recomputation.
    /// </summary>
    public bool PinnedOffsetsDirty { get; set; } = true;

    /// <summary>
    /// Whether any column is pinned at all, which is what makes placing them worth a call into JavaScript.
    /// </summary>
    public bool HasPinnedColumns => _columns.Exists(column => column.PinSide != PinSide.None);

    /// <summary>
    /// Whether the table has columns but shows none of them — what the column chooser leaves behind when the
    /// user hides the last one.
    /// </summary>
    public bool AllColumnsHidden => _columns.Count > 0 && !_columns.Exists(column => column.Visible);

    /// <summary>
    /// The column ids whose state the table may hold: the columns it has, minus the ones it hides. Deliberately
    /// the only definition of that set — a filter or sorting keyed to anything outside it has no header to
    /// display or clear it.
    /// </summary>
    public HashSet<string> ShownColumnIds => [.. _renderedColumns.Select(rendered => rendered.Column.Id)];

    /// <summary>
    /// How many columns the no-data placeholder has to span.
    /// </summary>
    /// <remarks>
    /// Counts the rendered columns only; counting the hidden ones too would stretch the placeholder past the
    /// table's own width. It never drops below one, because the column chooser may hide every column and
    /// <c>colspan="0"</c> is invalid HTML.
    /// </remarks>
    public int PlaceholderColumnSpan => Math.Max(1, _renderedColumns.Count);

    /// <summary>
    /// The width the table element itself is given.
    /// </summary>
    /// <remarks>
    /// The table is <c>width: 100%</c> while any flex column still awaits resolution (its <c>col</c> renders no
    /// width, CSS auto shares the space); once every column is pinned the summed px width takes over, so the
    /// table ends where the columns end (underflow leaves empty space to the right, overflow scrolls). Hidden
    /// columns render no <c>col</c>, so they are excluded here too — otherwise the table would keep a width
    /// wider than its visible columns and the browser would stretch them to fill it.
    /// </remarks>
    public string TableWidthStyle
    {
        get
        {
            if (HasUnresolvedFlexColumns)
                return "width: 100%";

            var total = _renderedColumns.Sum(rendered => GetEffectiveWidth(rendered.Column));

            return $"width: {(total ?? 0).ToAttributeValue(precision: 3)}px";
        }
    }

    private bool HasUnresolvedFlexColumns
        => _renderedColumns.Exists(rendered => GetEffectiveWidth(rendered.Column) is null);

    /// <summary>
    /// Registers <paramref name="column"/> and rebuilds the render order.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// A column with the same id is already registered. Checked against the incoming column before anything is
    /// mutated, so a rejected column is never left half-registered.
    /// </exception>
    public void Add(IAdvancedTableColumn<TItem> column)
    {
        List<string> duplicateColumnIds = [.. _columns
            .Append(column)
            .GroupBy(existing => existing.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)];

        if (duplicateColumnIds.Count != 0)
        {
            throw new ArgumentException(
                $"Received duplicate column id(s): {string.Join(", ", duplicateColumnIds)}. " +
                "Column Ids must be unique.");
        }

        _columns.Add(column);

        Rebuild();
    }

    /// <summary>
    /// Unregisters <paramref name="column"/>, drops the width remembered for it and rebuilds the render order.
    /// Returns whether the column was registered at all.
    /// </summary>
    public bool Remove(IAdvancedTableColumn<TItem> column)
    {
        if (!_columns.Remove(column))
            return false;

        _widthOverrides.Remove(column.Id);

        Rebuild();

        return true;
    }

    /// <summary>
    /// Moves <paramref name="column"/> to the gap beside <paramref name="target"/> — before it, or after it when
    /// <paramref name="after"/> is set — and rebuilds the render order.
    /// </summary>
    public void Move(IAdvancedTableColumn<TItem> column, IAdvancedTableColumn<TItem> target, bool after)
    {
        _columns.Remove(column);

        var insertIndex = _columns.IndexOf(target);

        if (after)
            insertIndex++;

        _columns.Insert(insertIndex, column);

        Rebuild();
    }

    /// <summary>
    /// The registered column carrying <paramref name="columnId"/>, or <see langword="null"/> when none does.
    /// </summary>
    public IAdvancedTableColumn<TItem>? Find(string columnId)
        => _columns.Find(column => column.Id == columnId);

    /// <summary>
    /// The column rendered directly after <paramref name="column"/>, or <see langword="null"/> when it is the
    /// last one rendered or is not rendered at all.
    /// </summary>
    public IAdvancedTableColumn<TItem>? GetNextRenderedColumn(IAdvancedTableColumn<TItem> column)
    {
        var nextIndex = _renderedColumns.FindIndex(rendered => rendered.Column == column) + 1;

        if (nextIndex <= 0 || nextIndex >= _renderedColumns.Count)
            return null;

        return _renderedColumns[nextIndex].Column;
    }

    /// <summary>
    /// Commits the pixel widths JavaScript computed, so they survive Blazor re-renders. Widths of columns no
    /// longer present are ignored. <paramref name="userFixedColumnId"/> names the column the user pinned by
    /// dragging its resize handle, if any. Returns whether anything was committed.
    /// </summary>
    public bool ApplyWidths(IReadOnlyCollection<ColumnWidth> columnWidths, string? userFixedColumnId)
    {
        var applied = false;

        foreach (var columnWidth in columnWidths)
        {
            if (Find(columnWidth.ColumnId) is null)
                continue;

            var origin = columnWidth.ColumnId == userFixedColumnId
                ? ColumnWidthOrigin.UserFixed
                : ColumnWidthOrigin.Resolved;

            // A width the browser resolved never overwrites one the user chose deliberately.
            if (origin == ColumnWidthOrigin.Resolved && IsUserFixed(columnWidth.ColumnId))
                continue;

            _widthOverrides[columnWidth.ColumnId] = new ColumnWidthOverride(columnWidth.Value, origin);

            PinnedOffsetsDirty = true;
            applied = true;
        }

        return applied;
    }

    /// <summary>
    /// The attributes the <c>col</c> element of <paramref name="column"/> carries: its id, the minimum width a
    /// resize may not go below, a marker for a column still sharing the leftover space, and its resolved width.
    /// </summary>
    public Dictionary<string, object> GetColumnAttributes(IAdvancedTableColumn<TItem> column)
    {
        var attributes = new Dictionary<string, object> { { "data-column-id", column.Id } };

        if (column is IResizeableColumn resizeableColumn)
            attributes.Add("data-min-width", resizeableColumn.MinimumWidth);

        if (IsFlexColumn(column))
            attributes.Add("data-flex", string.Empty);

        if (GetEffectiveWidth(column) is { } width)
            attributes.Add("style", $"width: {width.ToAttributeValue(precision: 3)}px");

        return attributes;
    }

    /// <summary>
    /// Recomputes the render order and the pin placement. Needed from outside whenever a column changed its own
    /// visibility or pin side, because those reach the layout through the column rather than through it.
    /// </summary>
    public void Rebuild()
    {
        _orderedColumns = [.. _columns.OrderBy(GetPinOrder)];

        _renderedColumns = [.. _orderedColumns
            .Where(column => column.Visible)
            .Select((column, index) => new RenderedColumn<TItem>(column, GetPinStyle(column, index)))];

        PinnedOffsetsDirty = true;
    }

    private double? GetEffectiveWidth(IAdvancedTableColumn<TItem> column)
    {
        if (_widthOverrides.TryGetValue(column.Id, out var columnWidth))
            return columnWidth.Value;

        return column.DefaultWidth;
    }

    private bool IsFlexColumn(IAdvancedTableColumn<TItem> column)
        => column.DefaultWidth is null && !IsUserFixed(column.Id);

    private bool IsUserFixed(string columnId)
        => _widthOverrides.TryGetValue(columnId, out var columnWidth) && columnWidth.Origin == ColumnWidthOrigin.UserFixed;

    private static int GetPinOrder(IAdvancedTableColumn<TItem> column)
        => column.PinSide switch
        {
            PinSide.Left => 0,
            PinSide.Right => 2,
            _ => 1
        };

    // updatePinnedOffsets in JavaScript fills these custom properties, counting the headers off in the same order.
    private static string? GetPinStyle(IAdvancedTableColumn<TItem> column, int index)
        => column.PinSide switch
        {
            PinSide.Left => $"left: var(--pin-left-{index})",
            PinSide.Right => $"right: var(--pin-right-{index})",
            _ => null
        };
}
