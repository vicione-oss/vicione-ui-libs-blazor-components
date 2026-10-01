using Microsoft.AspNetCore.Components;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Components;

public sealed partial class SimpleTableRowsDragAndDropPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(10);

    private List<ExampleTableItem> _selectedItems = [];

    [Inject]
    private VisibleSelectionStore VisibleSelection { get; set; } = default!;

    [Inject]
    private ITableDragPayloadProvider<ExampleTableItem> DragPayloadProvider { get; set; } = default!;
}
