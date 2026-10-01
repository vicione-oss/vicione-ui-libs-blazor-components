using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Reflow.Components;

public sealed partial class AdvancedTableReflowPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    private bool _narrow;

    private string GetContainerCssClass(string sampleCssClass)
    {
        List<string> cssClasses = ["reflow-container", sampleCssClass];

        if (_narrow)
            cssClasses.Add("narrow");

        return string.Join(" ", cssClasses);
    }
}
