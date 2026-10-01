using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// Base class for all components rendered as header cell content within a <see cref="AdvancedTable{TItem}"/>.
/// Provides access to the contextual state of the column the header cell belongs to.
/// </summary>
/// <remarks>
/// A header cell belongs to no row, so unlike <see cref="TableBodyCellContentBase{TItem}"/> it offers no row state.
/// </remarks>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
public abstract class TableHeaderCellContentBase<TItem> : ComponentBase
    where TItem : class
{
    /// <summary>
    /// The shared state of the column containing this header cell.
    /// Used to access or trigger column-specific things.
    /// </summary>
    [CascadingParameter]
    internal ColumnState ColumnState { get; set; } = default!;
}
