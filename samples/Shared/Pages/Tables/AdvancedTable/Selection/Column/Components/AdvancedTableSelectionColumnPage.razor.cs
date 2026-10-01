using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Selection.Column.Components;

public sealed partial class AdvancedTableSelectionColumnPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    private List<ExampleTableItem> _bothChannelsSingleSelection = [];
    private List<ExampleTableItem> _bothChannelsMultipleSelection = [];
    private List<ExampleTableItem> _columnOnlySingleSelection = [];
    private List<ExampleTableItem> _columnOnlyMultipleSelection = [];
}
