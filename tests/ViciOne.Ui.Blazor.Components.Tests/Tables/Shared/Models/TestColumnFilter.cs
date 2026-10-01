using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

internal sealed class TestColumnFilter(string columnId = "TestColumn") : IColumnFilter
{
    public string ColumnId => columnId;
}
