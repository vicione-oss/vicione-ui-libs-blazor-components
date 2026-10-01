using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

// Claims both scopes, which is never meaningful: a column filter is keyed by its column id and a global filter
// by its CLR type, so one instance would occupy both keyspaces. Exists only to reach WithGlobalFilter's guard —
// the one path the signature cannot close, because C# cannot declare two interfaces mutually exclusive.
internal sealed class TestColumnAndGlobalFilter(string columnId) : IColumnFilter, IGlobalFilter
{
    public string ColumnId => columnId;
}
