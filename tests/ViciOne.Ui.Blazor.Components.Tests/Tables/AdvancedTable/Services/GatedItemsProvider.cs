using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;

// Withholds its response until the gate is released, so a test can observe the table while it is still
// waiting for its first items.
internal sealed class GatedItemsProvider(List<TableTestItem> items) : IItemsProvider<TableTestItem>
{
    private readonly TaskCompletionSource _gate = new();

    public void Release()
        => _gate.TrySetResult();

    public async Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
        CancellationToken cancellationToken)
    {
        await _gate.Task.WaitAsync(cancellationToken);

        return new ItemsProviderResult<TableTestItem>(items, items.Count);
    }
}
