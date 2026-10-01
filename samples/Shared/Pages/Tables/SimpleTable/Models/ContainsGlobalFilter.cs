using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace Shared.Pages.Tables.SimpleTable.Models;

// A consumer-authored global filter: a whole-item OR predicate with no column id, driven from a search box
// outside the table. The table never sees the text — only the Matches result — so the box's backing value is
// the consumer's own concern.
internal sealed class ContainsGlobalFilter(string text) : IGlobalFilter, ISimpleTableFilter<ExampleTableItem>
{
    public bool Matches(ExampleTableItem item)
        => item.Key.Contains(text, StringComparison.OrdinalIgnoreCase)
            || item.Value.Contains(text, StringComparison.OrdinalIgnoreCase);
}
