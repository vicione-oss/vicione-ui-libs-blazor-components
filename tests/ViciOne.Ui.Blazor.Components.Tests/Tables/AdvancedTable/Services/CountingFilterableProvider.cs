using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;

// Counts provides and hides the last item whenever any filter is active, so a test can tell a
// filter-driven reload apart from a data reload.
internal sealed class CountingFilterableProvider(List<TableTestItem> allItems)
    : IItemsProvider<TableTestItem>
{
    public int CallCount { get; private set; }

    public Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
        CancellationToken cancellationToken)
    {
        CallCount++;

        var items = context.FilterState.Filters.Count > 0 ? allItems.Take(allItems.Count - 1) : allItems;

        return Task.FromResult(new ItemsProviderResult<TableTestItem>([.. items], allItems.Count));
    }
}
