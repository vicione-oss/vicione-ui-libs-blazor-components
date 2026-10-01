using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Cascaded over a column's filter editor while its panel is open. Carries everything a filter editor component
/// needs to read the active filter and signal changes back to the table.
/// </summary>
/// <param name="ColumnId">The id of the column the filter editor belongs to.</param>
/// <param name="ColumnFilter">
/// The filter currently active on the column, or <see langword="null"/> when the column is unfiltered.
/// </param>
/// <param name="ColumnFilterChanged">
/// Invoked to apply a filter to this one column, keyed by <paramref name="ColumnId"/>. Pass
/// <see langword="null"/> to clear that column's filter. The table folds the result into its own filter state
/// and leaves every other column's filter untouched, so this is not a way to set the whole filter state.
/// </param>
/// <param name="CloseRequested">Invoked to close the filter panel without changing the filter.</param>
public sealed record ColumnFilterEditorContext(
    string ColumnId,
    IColumnFilter? ColumnFilter,
    EventCallback<IColumnFilter?> ColumnFilterChanged,
    EventCallback CloseRequested);
