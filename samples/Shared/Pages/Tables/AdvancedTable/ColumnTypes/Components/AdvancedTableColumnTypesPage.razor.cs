using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.ColumnTypes.Components;

public sealed partial class AdvancedTableColumnTypesPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());
}
