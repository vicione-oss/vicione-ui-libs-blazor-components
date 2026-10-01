namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// The complete set of filters of a table, round-trippable for
/// two-way binding.
/// </summary>
/// <remarks>
/// <para>One value, not a container of values: reference equality is the change signal and
/// <see cref="Empty"/> is a singleton. Use <see cref="Empty"/> as the initial value and the
/// <c>With…</c>/<c>Without…</c> helpers to produce updated instances. Each filter is either scoped to a
/// column (<see cref="IColumnFilter"/>, keyed by column id) or global (<see cref="IGlobalFilter"/>, keyed by
/// its CLR type). The private constructor makes a duplicate-column collection unconstructable.</para>
/// <para>A helper that would change nothing returns the same instance, and emptying one returns
/// <see cref="Empty"/>. The table compares by reference to decide whether anything happened.</para>
/// <para>Says nothing about whether the table has applied it — which role a given value plays is named by
/// the member holding it.</para>
/// </remarks>
public sealed record FilterState
{
    /// <summary>
    /// An empty filter state with no active filters.
    /// </summary>
    public static readonly FilterState Empty = new([]);

    /// <summary>
    /// The active filters — column-scoped and global, in insertion order.
    /// </summary>
    public IReadOnlyList<IFilter> Filters { get; }

    private FilterState(IReadOnlyList<IFilter> filters)
        => Filters = filters;

    /// <summary>
    /// Returns a <see cref="FilterState"/> with <paramref name="filter"/> set for its column, replacing any
    /// existing filter for the same column and leaving all other entries untouched. Returns the same instance
    /// when that column already carries an equal filter.
    /// </summary>
    public FilterState WithColumnFilter(IColumnFilter filter)
    {
        var existingColumnFilterIndex = GetColumnFilterIndex(filter.ColumnId);

        // Equals rather than ==: the static type is an interface, so == would compare references and miss a
        // filter that defines value equality.
        if (existingColumnFilterIndex >= 0 && Filters[existingColumnFilterIndex].Equals(filter))
            return this;

        return new([.. Filters.Where(existing => existing is not IColumnFilter column ||
            column.ColumnId != filter.ColumnId), filter]);
    }

    /// <summary>
    /// Returns a <see cref="FilterState"/> with the filter for <paramref name="columnId"/> removed, leaving
    /// global filters and other columns' filters untouched. Returns the same instance when that column carries
    /// no filter.
    /// </summary>
    public FilterState WithoutColumnFilter(string columnId)
    {
        if (GetColumnFilterIndex(columnId) < 0)
            return this;

        return Create([.. Filters.Where(existing => existing is not IColumnFilter column ||
            column.ColumnId != columnId)]);
    }

    /// <summary>
    /// Returns a <see cref="FilterState"/> with <paramref name="filter"/> set as a global filter, replacing
    /// any existing global filter of the same CLR type. A global filter carries no column id; it is keyed by
    /// its type, so two different global-filter types both stay active and compose. Returns the same instance
    /// when an equal global filter of that type is already active.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="filter"/> is also an <see cref="IColumnFilter"/>. Column filters are keyed by column
    /// id, so adding one here would key it by type instead and drop every column's filter of that type.
    /// </exception>
    public FilterState WithGlobalFilter(IGlobalFilter filter)
    {
        if (filter is IColumnFilter)
        {
            throw new ArgumentException(
                $"A column filter must be added with {nameof(WithColumnFilter)} so it is keyed by its column id.",
                nameof(filter));
        }

        var filterType = filter.GetType();

        if (Filters.Any(existing => existing.GetType() == filterType && existing.Equals(filter)))
            return this;

        return new([.. Filters.Where(existing => existing.GetType() != filterType), filter]);
    }

    /// <summary>
    /// Returns a <see cref="FilterState"/> with the global filter of type <typeparamref name="T"/> removed, or
    /// the same instance when no such filter is active.
    /// </summary>
    /// <typeparam name="T">The global filter type to remove.</typeparam>
    public FilterState WithoutGlobalFilter<T>()
        where T : IGlobalFilter
    {
        if (!Filters.OfType<T>().Any())
            return this;

        return Create([.. Filters.Where(existing => existing is not T)]);
    }

    // "No filter at all" must be one instance, or removing the last entry yields a value indistinguishable
    // from Empty that still compares as a change.
    private static FilterState Create(IReadOnlyList<IFilter> filters)
    {
        if (filters.Count == 0)
            return Empty;

        return new(filters);
    }

    private int GetColumnFilterIndex(string columnId)
    {
        for (var i = 0; i < Filters.Count; i++)
        {
            if (Filters[i] is IColumnFilter columnFilter && columnFilter.ColumnId == columnId)
                return i;
        }

        return -1;
    }
}
