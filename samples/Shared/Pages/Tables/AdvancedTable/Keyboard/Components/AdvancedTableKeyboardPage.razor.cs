using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Keyboard.Components;

public sealed partial class AdvancedTableKeyboardPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    // Enough rows that the rendered window is a small slice of the set, which is what makes a focused cell
    // scrolled out of that window — and brought back again — reachable by hand.
    private static readonly List<ExampleTableItem> s_manyItems = ExampleTableItemFactory.CreateMany(900);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    private readonly IItemsProvider<ExampleTableItem> _manyItemsProvider =
        new ExampleTableItemsProvider(s_manyItems.AsQueryable());

    private readonly IItemsProvider<ExampleTableItem> _emptyItemsProvider =
        new ExampleTableItemsProvider(new List<ExampleTableItem>().AsQueryable());

    private List<ExampleTableItem> _multipleSelectedItems = [];

    private List<ExampleTableItem> _singleSelectedItems = [];

    private List<ExampleTableItem> _virtualizedSelectedItems = [];

    private string? _lastActivatedKey;

    private bool _valueColumnVisible = true;

    // Vetoes one row by position rather than by content, so the row the veto covers is the same one on every
    // load even though the rows themselves are generated.
    private static bool ThirdRowVetoed(SelectionRequest<ExampleTableItem> request)
        => s_items.IndexOf(request.Item) != 2;

    private void ToggleValueColumn()
        => _valueColumnVisible = !_valueColumnVisible;

    private Task ActivateAsync(ExampleTableItem item)
    {
        _lastActivatedKey = item.Key;

        return Task.CompletedTask;
    }
}
