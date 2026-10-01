using Microsoft.AspNetCore.Components;
using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using Shared.Pages.Tables.Shared.Rows.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Rows.ContextMenu.Components;

public sealed partial class AdvancedTableRowsContextMenuPage
{
    // Quantity from which a row counts as high-quantity, splitting the sample rows into the two kinds
    // the menu entries declare themselves applicable to.
    private const int HighQuantityThreshold = 100;

    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private string? _lastAction;

    private List<ExampleTableItem> _selectedItems = [];

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    [Inject]
    private IContextMenuRequest<ExampleRowContextMenuContext> ContextMenuRequest { get; set; } = default!;

    [Inject]
    private IContextMenuSettings ContextMenuSettings { get; set; } = default!;

    // The kinds present in the given rows. An entry shows only while its ApplicableTo covers all of them,
    // so a mixed selection drops the entries that handle just one kind.
    private static List<Type> GetRowKinds(IReadOnlyList<ExampleTableItem> items)
    {
        var kinds = new List<Type>();

        if (items.Any(item => item.Quantity >= HighQuantityThreshold))
            kinds.Add(typeof(HighQuantityRow));

        if (items.Any(item => item.Quantity < HighQuantityThreshold))
            kinds.Add(typeof(LowQuantityRow));

        return kinds;
    }

    private async Task RowRightClickAsync(RowContextMenuEventArgs<ExampleTableItem> args)
    {
        // The table reports every right-click, whether or not custom menus are switched on. With them off
        // the browser opens its own menu and SendAsync does nothing, so the selection must be left alone
        // too — moving it here would collapse the selection under a menu that knows nothing about it.
        if (!ContextMenuSettings.UseCustomMenu)
            return;

        // A right-click outside the selection replaces it, so the highlight and the menu agree on what an
        // action would apply to. Inside the selection it is left alone and the menu acts on all of it.
        if (!_selectedItems.Contains(args.Item))
            _selectedItems = [args.Item];

        await ContextMenuRequest.SendAsync(new ExampleRowContextMenuContext
        {
            Items = _selectedItems,
            ItemFilter = new ContextMenuItemFilter { ApplicableTo = GetRowKinds(_selectedItems) },
            MouseEventArgs = args.MouseEventArgs
        });
    }
}
