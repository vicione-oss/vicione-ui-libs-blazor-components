using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;

internal sealed class RangedItemsProvider(List<TableTestItem> items) : IItemsProvider<TableTestItem>
{
    private readonly List<ItemsProviderContext> _receivedContexts = [];

    public IReadOnlyList<ItemsProviderContext> ReceivedContexts => _receivedContexts;

    public int CallCount => _receivedContexts.Count;

    public Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
        CancellationToken cancellationToken)
    {
        _receivedContexts.Add(context);

        IEnumerable<TableTestItem> result = items;

        if (context.ItemRange is { } itemRange)
            result = result.Skip(itemRange.Skip).Take(itemRange.Take);

        return Task.FromResult(new ItemsProviderResult<TableTestItem>([.. result], items.Count));
    }
}
