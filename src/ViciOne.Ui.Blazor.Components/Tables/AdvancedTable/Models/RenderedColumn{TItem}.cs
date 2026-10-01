using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

internal readonly record struct RenderedColumn<TItem>(IAdvancedTableColumn<TItem> Column, string? PinStyle)
    where TItem : class;
