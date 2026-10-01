using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Reflow.Components;

public sealed partial class SimpleTableReflowPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private bool _narrow;

    private string GetContainerCssClass(string sampleCssClass)
    {
        List<string> cssClasses = ["reflow-container", sampleCssClass];

        if (_narrow)
            cssClasses.Add("narrow");

        return string.Join(" ", cssClasses);
    }
}
