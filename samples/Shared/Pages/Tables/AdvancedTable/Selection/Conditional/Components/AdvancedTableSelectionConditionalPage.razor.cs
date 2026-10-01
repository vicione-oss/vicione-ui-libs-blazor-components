using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Selection.Conditional.Components;

public sealed partial class AdvancedTableSelectionConditionalPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    private List<ExampleTableItem> _conditionalSelectedItems = [];

    private static bool ConditionalItemSelectionAllowed(SelectionRequest<ExampleTableItem> req)
        => string.Compare(req.Item.Key, "n", StringComparison.OrdinalIgnoreCase) < 0;
}
