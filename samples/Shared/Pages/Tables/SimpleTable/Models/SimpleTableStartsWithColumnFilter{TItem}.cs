using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace Shared.Pages.Tables.SimpleTable.Models;

public sealed record SimpleTableStartsWithColumnFilter<TItem>(string ColumnId, string Value, Func<TItem, string?> ValueSelector)
    : ISimpleTableColumnFilter<TItem>
        where TItem : class
{
    public bool Matches(TItem item)
    {
        var value = ValueSelector(item);

        return value?.StartsWith(Value, StringComparison.OrdinalIgnoreCase) == true;
    }
}
