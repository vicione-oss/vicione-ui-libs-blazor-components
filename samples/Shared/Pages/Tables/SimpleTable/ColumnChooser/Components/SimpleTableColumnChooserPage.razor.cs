using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.ColumnChooser.Components;

public sealed partial class SimpleTableColumnChooserPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly object _columnChooserToggleId = new();
}
