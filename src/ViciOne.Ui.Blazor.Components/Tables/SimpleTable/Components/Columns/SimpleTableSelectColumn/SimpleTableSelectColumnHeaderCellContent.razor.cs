using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;

/// <summary>
/// Header cell content of a <see cref="SimpleTableSelectColumn{TItem}"/>.
/// </summary>
public sealed partial class SimpleTableSelectColumnHeaderCellContent<TItem> : TableHeaderCellContentBase<TItem>
    where TItem : class
{
    private bool AllowIndeterminateState => Value is null;

    /// <summary>
    /// Whether the header is enabled and usable or not.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; }

    /// <summary>
    /// Value of the Header.
    ///
    /// <para>If <see langword="true"/>, all rows are selected.</para>
    /// <para>If <see langword="false"/> none are selected.</para>
    /// <para>If <see langword="null"/> some are selected.</para>
    /// </summary>
    [Parameter]
    public bool? Value { get; set; }

    /// <summary>
    /// Raised when <see cref="Value" /> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool?> ValueChanged { get; set; }

    private async Task ValueChangedAsync(bool? value)
    {
        Value = value;

        if (ValueChanged.HasDelegate)
            await ValueChanged.InvokeAsync(Value);

        ColumnState.RequestRefresh();
    }
}
