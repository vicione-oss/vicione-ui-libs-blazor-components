using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Footer.Components;

public sealed partial class AdvancedTableFooterPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _selectedItems = [];

    private List<ExampleTableItem> _bothCountsSelectedItems = [];

    private static IItemsProvider<ExampleTableItem> StaticItemsProvider => new ExampleTableItemsProvider(s_items.AsQueryable());
}
