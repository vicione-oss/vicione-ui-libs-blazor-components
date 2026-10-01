using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Selection.DisplayOnly.Components;

public sealed partial class SimpleTableSelectionDisplayOnlyPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    // Each table starts with a host-supplied selection to show that display-only still renders it.
    private List<ExampleTableItem> _rowDisplayOnly = [.. s_items.Take(2)];
    private List<ExampleTableItem> _columnDisplayOnly = [.. s_items.Take(2)];
    private List<ExampleTableItem> _wholeTableDisplayOnly = [.. s_items.Take(2)];
}
