using System.Linq.Expressions;
using Shared.Pages.Tables.AdvancedTable.Models;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Localization.Extensions;

namespace Shared.Pages.Tables.AdvancedTable.Services;

internal sealed class ExampleTableItemsProvider(IQueryable<ExampleTableItem> items) : IItemsProvider<ExampleTableItem>
{
    public Task<ItemsProviderResult<ExampleTableItem>> GetItemsAsync(ItemsProviderContext context, CancellationToken cancellationToken)
    {
        var result = items;

        foreach (var filter in context.FilterState.Filters)
        {
            if (filter is StartsWithColumnFilter startsWithColumnFilter)
            {
                result = startsWithColumnFilter.ColumnId switch
                {
                    "Key" => result.Where(x => x.Key.StartsWith(startsWithColumnFilter.Value, StringComparison.OrdinalIgnoreCase)),
                    "Value" => result.Where(x => x.Value.StartsWith(startsWithColumnFilter.Value, StringComparison.OrdinalIgnoreCase)),
                    // Compares the text the Timestamp cell renders, so what the user filters on is what the
                    // user sees. Keep this format and the filter page's FormatTimestamp in step.
                    "Timestamp" => result.Where(x =>
                        $"{x.Timestamp.LocalizeShortDate()} {x.Timestamp.LocalizeShortTime()}"
                            .StartsWith(startsWithColumnFilter.Value, StringComparison.OrdinalIgnoreCase)),
                    _ => result
                };
            }
            else if (filter is IntRangeColumnFilter intRangeColumnFilter)
            {
                result = intRangeColumnFilter.ColumnId switch
                {
                    // Both bounds inclusive; a bound left null is unbounded on that side.
                    "Quantity" => result.Where(x =>
                        (intRangeColumnFilter.From == null || x.Quantity >= intRangeColumnFilter.From.Value)
                            && (intRangeColumnFilter.To == null || x.Quantity <= intRangeColumnFilter.To.Value)),
                    _ => result
                };
            }
        }

        // Post-filter, pre-paging count — this is what the footer and Virtualize extent must reflect.
        var totalCount = result.Count();

        IOrderedQueryable<ExampleTableItem>? sortedResult = null;

        // The entries come in priority order, so each one refines the ordering built so far.
        foreach (var columnSorting in context.SortingState.ColumnSortings)
        {
            var keySelector = GetSortKeySelector(columnSorting.ColumnId);

            if (keySelector is null)
                continue;

            if (sortedResult is null)
            {
                if (columnSorting.Ascending)
                    sortedResult = result.OrderBy(keySelector);
                else
                    sortedResult = result.OrderByDescending(keySelector);

                continue;
            }

            if (columnSorting.Ascending)
                sortedResult = sortedResult.ThenBy(keySelector);
            else
                sortedResult = sortedResult.ThenByDescending(keySelector);
        }

        if (sortedResult is not null)
            result = sortedResult;

        if (context.ItemRange is not null)
        {
            if (context.ItemRange.Skip != 0)
                result = result.Skip(context.ItemRange.Skip);

            result = result.Take(context.ItemRange.Take);
        }

        return Task.FromResult(new ItemsProviderResult<ExampleTableItem>([.. result], totalCount));
    }

    private static Expression<Func<ExampleTableItem, object>>? GetSortKeySelector(string columnId)
        => columnId switch
        {
            "Id" => item => item.Id,
            "Key" => item => item.Key,
            "Value" => item => item.Value,
            "Timestamp" => item => item.Timestamp,
            _ => null
        };
}
