using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// Base class for all components rendered as body cell content within a <see cref="AdvancedTable{TItem}"/>.
/// Header cell content derives from <see cref="TableHeaderCellContentBase{TItem}"/> instead.
/// Provides access to the contextual state of the row and column the cell belongs to.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
public abstract class TableBodyCellContentBase<TItem> : ComponentBase
    where TItem : class
{
    /// <summary>
    /// The shared state of the row containing this cell.
    /// Used to access or trigger row-specific things.
    /// </summary>
    [CascadingParameter]
    internal RowState RowState { get; set; } = default!;

    /// <summary>
    /// The shared state of the column containing this cell.
    /// Used to access or trigger column-specific things.
    /// </summary>
    [CascadingParameter]
    internal ColumnState ColumnState { get; set; } = default!;
}
