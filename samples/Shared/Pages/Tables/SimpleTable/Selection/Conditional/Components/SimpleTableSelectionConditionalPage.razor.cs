using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Selection.Conditional.Components;

public sealed partial class SimpleTableSelectionConditionalPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _conditionalSelectedItems = [];

    private static bool ConditionalCanSelect(SelectionRequest<ExampleTableItem> req)
        => string.Compare(req.Item.Key, "n", StringComparison.OrdinalIgnoreCase) < 0;
}
