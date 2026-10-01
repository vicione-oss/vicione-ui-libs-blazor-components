using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Selection.Column.Components;

public sealed partial class SimpleTableSelectionColumnPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _bothChannelsSingleSelection = [];
    private List<ExampleTableItem> _bothChannelsMultipleSelection = [];
    private List<ExampleTableItem> _filteredBulkSelection = [];
    private List<ExampleTableItem> _columnOnlySingleSelection = [];
    private List<ExampleTableItem> _columnOnlyMultipleSelection = [];
    private List<ExampleTableItem> _columnOnlyNoHeaderSelection = [];
}
