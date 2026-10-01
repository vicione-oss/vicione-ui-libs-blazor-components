using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Selection.RowClick.Components;

public sealed partial class SimpleTableSelectionRowClickPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _singleSelectedItems = [];
    private List<ExampleTableItem> _multiSelectedItems = [];
}
