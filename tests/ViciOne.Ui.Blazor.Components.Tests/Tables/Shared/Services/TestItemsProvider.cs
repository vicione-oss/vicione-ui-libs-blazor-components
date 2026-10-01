using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

internal sealed class TestItemsProvider(List<TableTestItem> items) : IItemsProvider<TableTestItem>
{
    public Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
        CancellationToken cancellationToken)
            => Task.FromResult(new ItemsProviderResult<TableTestItem>(items, items.Count));
}
