using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.PinnedColumns.Components;

public sealed partial class SimpleTablePinnedColumnsPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);
}
