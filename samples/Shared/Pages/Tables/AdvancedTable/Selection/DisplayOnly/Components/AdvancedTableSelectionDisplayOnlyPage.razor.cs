using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Selection.DisplayOnly.Components;

public sealed partial class AdvancedTableSelectionDisplayOnlyPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    // Each table starts with a host-supplied selection to show that display-only still renders it.
    private List<ExampleTableItem> _rowDisplayOnly = [.. s_items.Take(2)];
    private List<ExampleTableItem> _columnDisplayOnly = [.. s_items.Take(2)];
    private List<ExampleTableItem> _wholeTableDisplayOnly = [.. s_items.Take(2)];
}
