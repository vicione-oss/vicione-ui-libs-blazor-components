using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;

internal sealed class RecordingItemsProvider(List<TableTestItem> items, int? totalItemCount = null)
    : IItemsProvider<TableTestItem>
{
    public ItemsProviderContext? LastContext { get; private set; }

    public int CallCount { get; private set; }

    public Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
        CancellationToken cancellationToken)
    {
        LastContext = context;
        CallCount++;

        return Task.FromResult(new ItemsProviderResult<TableTestItem>(items, totalItemCount ?? items.Count));
    }
}
