using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.AdvancedTable.Models;

public sealed record StartsWithColumnFilter(string ColumnId, string Value) : IColumnFilter;
