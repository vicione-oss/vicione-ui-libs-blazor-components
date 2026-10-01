using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Selection.VisibleSelection.Components;

public sealed partial class SimpleTableSelectionVisibleSelectionPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private List<ExampleTableItem> _selectedItems = [];

    private IReadOnlyList<ExampleTableItem> _filteredItems = [];

    private IReadOnlyList<ExampleTableItem> _visibleSelection = [];

    private void StoreFilteredItems(IReadOnlyList<ExampleTableItem> filteredItems)
        => _filteredItems = filteredItems;

    private void StoreVisibleSelection(IReadOnlyList<ExampleTableItem> visibleSelection)
        => _visibleSelection = visibleSelection;
}
