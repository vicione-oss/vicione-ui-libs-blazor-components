using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace Shared.Pages.Tables.SimpleTable.Rows.Striping.Components;

public sealed partial class SimpleTableRowsStripingPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private bool _virtualize = true;

    private TableLoadingMode LoadingMode => _virtualize ? TableLoadingMode.Virtualize : TableLoadingMode.All;
}
