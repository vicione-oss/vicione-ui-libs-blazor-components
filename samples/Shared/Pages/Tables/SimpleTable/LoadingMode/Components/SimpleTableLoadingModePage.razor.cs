using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace Shared.Pages.Tables.SimpleTable.LoadingMode.Components;

public sealed partial class SimpleTableLoadingModePage
{
    // Large enough that a range selection reaches well beyond the rows the virtualized viewport holds.
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(500);

    private bool _virtualize = true;

    private List<ExampleTableItem> _selectedItems = [];

    private TableLoadingMode LoadingMode => _virtualize ? TableLoadingMode.Virtualize : TableLoadingMode.All;
}
