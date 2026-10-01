using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Localization.Extensions;

namespace Shared.Pages.Tables.SimpleTable.Filter.BuiltIn.Components;

public sealed partial class SimpleTableFilterBuiltInPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    // One definition for both the Timestamp cell and that column's filter projection. A Contains filter matches
    // the text the cell renders, so the two must produce the identical string.
    private static string FormatTimestamp(ExampleTableItem item)
        => $"{item.Timestamp.LocalizeShortDate()} {item.Timestamp.LocalizeShortTime()}";
}
