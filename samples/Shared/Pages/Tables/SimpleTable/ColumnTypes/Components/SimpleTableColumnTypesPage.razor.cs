using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.ColumnTypes.Components;

public sealed partial class SimpleTableColumnTypesPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _selectedItems = [];
}
