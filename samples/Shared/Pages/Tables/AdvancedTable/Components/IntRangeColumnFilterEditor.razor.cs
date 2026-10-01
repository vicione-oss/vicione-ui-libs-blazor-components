using Shared.Pages.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.AdvancedTable.Components;

// Two draft values, neither of them "the" value. Both live here rather than on the base, which is what lets one
// editor hold a pair while the text editors next door hold a single string.
public sealed partial class IntRangeColumnFilterEditor
{
    private int? _from;
    private int? _to;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Context.ColumnFilter is IntRangeColumnFilter rangeFilter)
        {
            _from = rangeFilter.From;
            _to = rangeFilter.To;
        }
    }

    protected override IColumnFilter? BuildFilter()
    {
        // Neither bound entered means no constraint, so the column's filter is cleared rather than replaced by a
        // filter that matches every row.
        if (_from is null && _to is null)
            return null;

        return new IntRangeColumnFilter(Context.ColumnId, _from, _to);
    }

    private void FromChanged(int? from)
        => _from = from;

    private void ToChanged(int? to)
        => _to = to;
}
