using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;

/// <summary>
/// Body cell content of an <see cref="AdvancedTableSelectColumn{TItem}"/>.
/// </summary>
public sealed partial class AdvancedTableSelectColumnBodyCellContent<TItem> : TableBodyCellContentBase<TItem>
    where TItem : class
{
    /// <summary>
    /// The item associated with the current row.
    /// This instance is used to identify the item within the selection collection.
    /// </summary>
    [Parameter, EditorRequired]
    public required TItem Item { get; set; }

    /// <summary>
    /// Indicates whether the current row is selected.
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// Whether the checkbox is enabled. Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The checkbox will be disabled when <see cref="AdvancedTable{TItem}.ItemSelectionAllowed"/> is set and rejects the item.
    /// </remarks>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Occurs when the selection state of the cell is toggled.
    /// Typically used for two-way binding with the <see cref="Value" /> property.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    private async Task ValueChangedAsync(bool value)
    {
        Value = value;

        try
        {
            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(Value);
        }
        finally
        {
            // Reconciles the local Value write above against the selection the table actually holds, so a
            // throwing consumer handler cannot leave the checkbox showing a selection that was never made.
            ColumnState.RequestRefresh();
        }
    }
}
