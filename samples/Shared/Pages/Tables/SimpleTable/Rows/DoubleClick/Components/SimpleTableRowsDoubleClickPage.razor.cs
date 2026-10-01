using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Rows.DoubleClick.Components;

public sealed partial class SimpleTableRowsDoubleClickPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(10);

    private ExampleTableItem? _doubleClickedItem;
}
