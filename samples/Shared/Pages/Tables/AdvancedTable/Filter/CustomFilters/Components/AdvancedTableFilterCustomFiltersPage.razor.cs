using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Localization.Extensions;

namespace Shared.Pages.Tables.AdvancedTable.Filter.CustomFilters.Components;

public sealed partial class AdvancedTableFilterCustomFiltersPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    // One definition for the Timestamp cell, matched by the provider's Timestamp filter branch. A filter over a
    // rendered timestamp must compare the text the cell shows, so the two must produce the identical string.
    private static string FormatTimestamp(ExampleTableItem item)
        => $"{item.Timestamp.LocalizeShortDate()} {item.Timestamp.LocalizeShortTime()}";
}
