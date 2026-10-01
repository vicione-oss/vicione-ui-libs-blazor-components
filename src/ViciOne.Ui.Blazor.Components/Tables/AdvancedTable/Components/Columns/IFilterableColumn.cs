using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// A column that offers an in-header filter editor.
/// </summary>
internal interface IFilterableColumn
{
    /// <summary>
    /// The editor rendered inside the column's filter panel, or <see langword="null"/> when the column
    /// currently offers no filter.
    /// </summary>
    RenderFragment? FilterEditor { get; }
}
