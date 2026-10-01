using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

// Every provide runs on the renderer's synchronization context, so the mutable state below is only ever
// touched by one thread at a time and needs no synchronization of its own.
internal sealed partial class SimpleTableItemsProvider<TItem>(IReadOnlyList<TItem> items,
    Func<IReadOnlyCollection<IAdvancedTableColumn<TItem>>> columns, ILogger<SimpleTableItemsProvider<TItem>> logger,
    ISimpleTableSortComparerResolver<TItem> sortComparerResolver)
        : IItemsProvider<TItem>, IReturnsStableInstances
            where TItem : class
{
    // De-dup the wrong-type misconfiguration log per (column, filter type) so a standing misconfiguration
    // does not spam on every provide (mirrors AdvancedTable's one-shot selection-misconfiguration log).
    // A global filter carries no column id and is keyed by type alone.
    private readonly HashSet<(string? ColumnId, Type FilterType)> _loggedMisconfigurations = [];

    // Compiling an expression tree emits IL, which is far too expensive to repeat on every provide — and a
    // provide happens per Virtualize page and per filter change. The compiled selector is kept per column and
    // reused while that column keeps handing back the same expression instance; a parent re-render produces a
    // fresh tree and recompiles. Bounded by the number of sortable columns.
    private readonly Dictionary<string, (Expression<Func<TItem, object>> Expression, Func<TItem, object> KeySelector)>
        _sortKeySelectors = [];

    private IReadOnlyList<TItem> _filteredItems = [];

    /// <summary>
    /// Every row the active filter left, unsliced and unsorted, as of the last provide.
    /// Empty until the first provide has run. Its consumers (the select-all header and its tri-state) read it
    /// from here: the provide already materializes this list to count and slice it, so keeping it is one field
    /// write and never triggers a second provider run — under Virtualize the ranged window is what gets
    /// returned, while this stays the whole filtered set.
    /// </summary>
    internal IReadOnlyList<TItem> FilteredItems => _filteredItems;

    /// <inheritdoc/>
    public Task<ItemsProviderResult<TItem>> GetItemsAsync(ItemsProviderContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Filter first, materializing once: the filtered list is the single source for the count, the sort,
        // and the page. The count is post-filter and pre-viewport, so the footer and
        // the Virtualize extent reflect what the filter left, not the source size.
        var filteredItems = context.FilterState.Filters.Count > 0
            ? ApplyFiltering(items, context.FilterState)
            : items;

        _filteredItems = filteredItems;

        var totalItemCount = filteredItems.Count;

        IEnumerable<TItem> result = filteredItems;

        // Filtering and sorting are synchronous passes over the whole set, so the token is re-checked between
        // them: a superseded provide should not go on to sort a result nobody will read.
        cancellationToken.ThrowIfCancellationRequested();

        if (context.SortingState.ColumnSortings.Count > 0)
            result = ApplySorting(filteredItems, columns(), context.SortingState);

        if (context.ItemRange is not null)
            result = result.Skip(context.ItemRange.Skip).Take(context.ItemRange.Take);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new ItemsProviderResult<TItem>([.. result], totalItemCount));
    }

    // Every column filter reaching here names a column the table has: the table drops the rest before it
    // applies a filter at all, so no column-registration tolerance is needed.
    private IReadOnlyList<TItem> ApplyFiltering(IReadOnlyList<TItem> items, FilterState filterState)
    {
        List<ISimpleTableFilter<TItem>> simpleTableFilters = [];

        foreach (var filter in filterState.Filters)
        {
            if (filter is IColumnFilter columnFilter)
            {
                if (filter is ISimpleTableFilter<TItem> simpleTableFilter)
                    simpleTableFilters.Add(simpleTableFilter);
                else
                    LogMisconfiguredFilter(filter, columnFilter.ColumnId);

                continue;
            }

            // A global filter carries no column id and matches the whole item, so it applies ungated by
            // column registration.
            if (filter is IGlobalFilter and ISimpleTableFilter<TItem> globalSimpleTableFilter)
                simpleTableFilters.Add(globalSimpleTableFilter);
            else
                LogMisconfiguredFilter(filter, columnId: null);
        }

        if (simpleTableFilters.Count == 0)
            return items;

        return [.. items.Where(item => simpleTableFilters.All(predicate => predicate.Matches(item)))];
    }

    // A filter SimpleTable cannot apply in-memory is a misconfiguration (an editor emitting the wrong filter
    // type, or a filter written for a different item type). Log and skip rather than throw: an unhandled
    // exception on the user's Apply click would kill the Blazor circuit and the cause would not be obvious.
    private void LogMisconfiguredFilter(IFilter filter, string? columnId)
    {
        if (!_loggedMisconfigurations.Add((columnId, filter.GetType())))
            return;

        if (columnId is null)
        {
            GlobalFilterNotApplicable(logger, filter.GetType().Name, typeof(TItem).Name);

            return;
        }

        ColumnFilterNotApplicable(logger, columnId, filter.GetType().Name, typeof(TItem).Name);
    }

    private IEnumerable<TItem> ApplySorting(IReadOnlyList<TItem> items,
        IReadOnlyCollection<IAdvancedTableColumn<TItem>> columns, SortingState sortingState)
    {
        var appliedSortings = ResolveSortings(columns, sortingState);

        if (appliedSortings.Count == 0)
            return items;

        var source = appliedSortings[^1].Ascending
            ? items
            : items.Reverse();

        var primarySorting = appliedSortings[0];

        var sortedItems = primarySorting.Ascending
            ? source.OrderBy(primarySorting.KeySelector, primarySorting.Comparer)
            : source.OrderByDescending(primarySorting.KeySelector, primarySorting.Comparer);

        foreach (var appliedSorting in appliedSortings.Skip(1))
        {
            sortedItems = appliedSorting.Ascending
                ? sortedItems.ThenBy(appliedSorting.KeySelector, appliedSorting.Comparer)
                : sortedItems.ThenByDescending(appliedSorting.KeySelector, appliedSorting.Comparer);
        }

        return sortedItems;
    }

    private List<(Func<TItem, object> KeySelector, IComparer<object>? Comparer, bool Ascending)> ResolveSortings(
        IReadOnlyCollection<IAdvancedTableColumn<TItem>> columns, SortingState sortingState)
    {
        List<(Func<TItem, object> KeySelector, IComparer<object>? Comparer, bool Ascending)> appliedSortings = [];

        foreach (var columnSorting in sortingState.ColumnSortings)
        {
            // The table drops sortings for columns it does not have, so this only catches the window between a
            // column disappearing and that drop being applied.
            var column = columns.FirstOrDefault(x => x.Id == columnSorting.ColumnId);
            if (column is null)
                continue;

            if (column is not IHasSortExpression<TItem> sortableColumn || sortableColumn.SortExpression is null)
            {
                OrderByColumnFailed(logger, columnSorting.ColumnId);

                continue;
            }

            var keySelector = GetSortKeySelector(columnSorting.ColumnId, sortableColumn.SortExpression);

            var comparer = sortableColumn.SortComparer is { } sortComparer
                ? sortComparerResolver.Resolve(columnSorting.ColumnId, sortComparer, sortableColumn.SortExpression)
                : null;

            appliedSortings.Add((keySelector, comparer, columnSorting.Ascending));
        }

        return appliedSortings;
    }

    private Func<TItem, object> GetSortKeySelector(string columnId, Expression<Func<TItem, object>> sortExpression)
    {
        if (_sortKeySelectors.TryGetValue(columnId, out var cached) && cached.Expression == sortExpression)
            return cached.KeySelector;

        var keySelector = sortExpression.Compile();

        _sortKeySelectors[columnId] = (sortExpression, keySelector);

        return keySelector;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Global filter of type {FilterType} does not implement " +
        "ISimpleTableFilter<{ItemType}>. SimpleTable applies filters in-memory and can only use filters that " +
        "implement this interface; the filter is skipped.")]
    private static partial void GlobalFilterNotApplicable(ILogger logger, string filterType, string itemType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Filter for column '{ColumnId}' of type {FilterType} does not " +
        "implement ISimpleTableColumnFilter<{ItemType}>. SimpleTable applies filters in-memory and can only use " +
        "filters that implement this interface; the filter is skipped.")]
    private static partial void ColumnFilterNotApplicable(ILogger logger, string columnId, string filterType,
        string itemType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Trying to order by column with id {ColumnId} failed. " +
        "Column does not support sorting (either by type or by configuration).")]
    private static partial void OrderByColumnFailed(ILogger logger, string columnId);
}
