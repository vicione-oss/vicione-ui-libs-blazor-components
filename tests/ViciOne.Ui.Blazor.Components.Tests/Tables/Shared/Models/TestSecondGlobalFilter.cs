using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

// A second global filter type, distinct from TestGlobalFilter, to prove type-keyed dedup keeps two different
// global-filter types active together and that both are applied.
internal sealed class TestSecondGlobalFilter(int minKey)
    : IGlobalFilter, ISimpleTableFilter<TableTestItem>
{
    public bool Matches(TableTestItem item)
        => item.TestKey >= minKey;
}
