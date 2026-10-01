using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

// A global filter carrying an in-memory predicate: no column id, matching over the whole item. AdvancedTable
// reads it as data; SimpleTable's provider calls Matches.
internal sealed class TestGlobalFilter(string contains) : IGlobalFilter, ISimpleTableFilter<TableTestItem>
{
    public bool Matches(TableTestItem item)
        => item.TestValue.Contains(contains, StringComparison.OrdinalIgnoreCase);
}
