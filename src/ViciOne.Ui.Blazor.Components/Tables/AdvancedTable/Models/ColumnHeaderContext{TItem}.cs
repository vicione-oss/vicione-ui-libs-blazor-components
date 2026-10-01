using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// Everything a column header needs from the table it belongs to: the column itself, the state shared with its
/// cells, the JavaScript seam its filter panel positions through, and the reorder interaction its drag gestures
/// drive.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <param name="Column">The column the header stands for.</param>
/// <param name="State">The column's shared state, which its header content reads.</param>
/// <param name="JsSession">The table's JavaScript seam.</param>
/// <param name="Reorder">The drag-reorder interaction the header reports its gestures to directly.</param>
/// <param name="ReorderChanged">
/// Raised after a gesture changed the reorder state. The header calls <paramref name="Reorder"/> itself, which
/// would re-render that header alone — but the gap zones and the drop indicator belong to every other header
/// too, so the table has to draw the whole row again.
/// </param>
/// <remarks>
/// Cascaded as one object rather than as separate values: every part is an internal type, and a Razor component
/// is always generated public, so none of them can appear on a public parameter. One cascade also keeps the
/// header's declaration readable, where a stack of them buried it.
/// </remarks>
internal sealed record ColumnHeaderContext<TItem>(
    IAdvancedTableColumn<TItem> Column,
    ColumnState State,
    IAdvancedTableJsSession JsSession,
    ColumnReorderInteraction<TItem> Reorder,
    EventCallback ReorderChanged)
    where TItem : class;
