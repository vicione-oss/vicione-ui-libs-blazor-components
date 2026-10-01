using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Rows.DragAndDrop.Components;

public sealed partial class AdvancedTableRowsDragAndDropPage
{
    private static readonly List<ExampleTableItem> s_draggableItems = ExampleTableItemFactory.CreateMany(10);

    private List<ExampleTableItem> _selectedItems = [];

    private readonly IItemsProvider<ExampleTableItem> _draggableItemsProvider
        = new ExampleTableItemsProvider(s_draggableItems.AsQueryable());
}
