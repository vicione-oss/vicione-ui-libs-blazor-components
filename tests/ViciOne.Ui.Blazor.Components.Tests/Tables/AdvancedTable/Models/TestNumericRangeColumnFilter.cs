using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Models;

/// <summary>
/// A two-value column filter, to prove the editor base can host a draft that is not a single string. Both bounds
/// are inclusive; a <see langword="null"/> bound means unbounded on that side.
/// </summary>
internal sealed class TestNumericRangeColumnFilter(string columnId = "TestColumn", int? from = null, int? to = null)
    : IColumnFilter
{
    public string ColumnId => columnId;

    public int? From => from;

    public int? To => to;
}
