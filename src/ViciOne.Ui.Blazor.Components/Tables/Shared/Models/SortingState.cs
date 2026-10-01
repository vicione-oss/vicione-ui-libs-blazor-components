namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Represents the state of the sorting of a table.
/// </summary>
/// <remarks>
/// <para>Holds at most one <see cref="ColumnSorting"/> per column. Use <see cref="Empty"/> as the initial value
/// and <see cref="WithColumnSorting(ColumnSorting)"/>/<see cref="WithoutColumnSorting"/> to produce updated
/// instances. The private constructor makes a duplicate-column collection unconstructable.</para>
/// <para>The order of <see cref="ColumnSortings"/> is the sort priority: the first entry is the primary sort
/// and the rest chain after it. Replacing a column's entry therefore keeps its position.</para>
/// <para>A helper that would change nothing returns the same instance, and emptying one returns
/// <see cref="Empty"/>. The table compares by reference to decide whether anything happened.</para>
/// </remarks>
public sealed record SortingState
{
    /// <summary>
    /// An empty sorting with no active column sortings.
    /// </summary>
    public static readonly SortingState Empty = new([]);

    /// <summary>
    /// The columns the sorting is applied by, in priority order — the first entry is the primary sort.
    /// At most one entry per column.
    /// </summary>
    public IReadOnlyList<ColumnSorting> ColumnSortings { get; }

    private SortingState(IReadOnlyList<ColumnSorting> columnSortings)
        => ColumnSortings = columnSortings;

    /// <summary>
    /// Returns a <see cref="SortingState"/> with <paramref name="columnSorting"/> set for its column, replacing
    /// any existing sorting for that column in place. A column already in the list keeps its position, and
    /// therefore its sort priority — only its direction changes; a column not yet present is appended as the
    /// lowest priority. Returns the same instance when that column is already sorted that way.
    /// </summary>
    public SortingState WithColumnSorting(ColumnSorting columnSorting)
    {
        var existingColumSortingIndex = GetColumnSortingIndex(columnSorting.ColumnId);

        if (existingColumSortingIndex < 0)
            return new([.. ColumnSortings, columnSorting]);

        if (ColumnSortings[existingColumSortingIndex] == columnSorting)
            return this;

        List<ColumnSorting> columnSortings = [.. ColumnSortings];
        columnSortings[existingColumSortingIndex] = columnSorting;

        return new(columnSortings);
    }

    /// <summary>
    /// Returns a new <see cref="SortingState"/> sorted by <paramref name="columnId"/> in the given direction,
    /// replacing any existing sorting for the same column.
    /// </summary>
    public SortingState WithColumnSorting(string columnId, bool ascending)
        => WithColumnSorting(new ColumnSorting(columnId, ascending));

    /// <summary>
    /// Returns a <see cref="SortingState"/> with the sorting for <paramref name="columnId"/> removed, or the same
    /// instance when that column is not sorted.
    /// </summary>
    public SortingState WithoutColumnSorting(string columnId)
    {
        if (GetColumnSortingIndex(columnId) < 0)
            return this;

        var remaining = ColumnSortings.Where(columnSorting => columnSorting.ColumnId != columnId).ToList();

        // "Not sorted at all" must be one instance, or removing the last entry yields a value indistinguishable
        // from Empty that still compares as a change.
        if (remaining.Count == 0)
            return Empty;

        return new(remaining);
    }

    private int GetColumnSortingIndex(string columnId)
    {
        for (var i = 0; i < ColumnSortings.Count; i++)
        {
            if (ColumnSortings[i].ColumnId == columnId)
                return i;
        }

        return -1;
    }
}
