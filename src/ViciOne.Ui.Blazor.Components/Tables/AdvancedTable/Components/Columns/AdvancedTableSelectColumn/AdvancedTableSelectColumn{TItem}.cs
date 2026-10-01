using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;

/// <summary>
/// A column for the <see cref="AdvancedTable{TItem}"/> that provides row selection functionality.
/// </summary>
public class AdvancedTableSelectColumn<TItem> : AdvancedTableColumnBase<TItem>
    where TItem : class
{
    private readonly string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// When <see langword="true"/>, the column channel is display-only: the cell checkbox renders its
    /// checked state but disabled and cannot change the selection; the current <see cref="AdvancedTable{TItem}.SelectedItems"/>
    /// still renders. Defaults to <see langword="false"/>. Independent of the row-click channel.
    /// </summary>
    [Parameter]
    public bool DisplayOnly { get; set; }

    /// <summary>
    /// Creates a column whose chooser row is suppressed by default.
    /// </summary>
    /// <remarks>
    /// The column renders no header text, so a chooser row for it would be unlabeled and hiding it would hide
    /// selection. Markup can still opt back in.
    /// </remarks>
    public AdvancedTableSelectColumn()
        => ShowInColumnChooser = false;

    // Disabled when the column channel is display-only (DisplayOnly — the checked state still renders, but the
    // checkbox cannot mutate the selection). Otherwise disabled only for items that cannot join the selection:
    // a veto is configured, the item is not already selected, and it currently fails the veto.
    private bool IsCellEnabled(TItem item)
        => !DisplayOnly
            && (AdvancedTable.ItemSelectionAllowed is null
                || AdvancedTable.IsSelected(item)
                || AdvancedTable.ItemSelectionAllowed(new SelectionRequest<TItem>(item, AdvancedTable.SelectedItems)));

    private Task CellCheckBoxValueChangedAsync(bool value, TItem item)
    {
        // Display-only column channel: the cell self-gates here so a disabled checkbox that still manages to raise a
        // change (e.g. a forced event) cannot mutate the selection. The table's mutation methods carry no such guard.
        if (DisplayOnly)
            return Task.CompletedTask;

        if (!value)
            return AdvancedTable.DeselectAsync(item);

        // Single mode: atomically replace the selection so the previously selected row is dropped.
        return AdvancedTable.SelectionMode == SelectionMode.Single
            ? AdvancedTable.SelectSingleAsync(item)
            : AdvancedTable.SelectAsync(item);
    }

    /// <inheritdoc/>
    private protected override string GetColumnId()
        => _id;

    /// <inheritdoc/>
    private protected override RenderFragment GetHeader()
        => _ => { };

    /// <inheritdoc/>
    private protected override RenderFragment<TItem> GetCellContent()
        => item => builder =>
        {
            builder.OpenComponent<AdvancedTableSelectColumnBodyCellContent<TItem>>(1);
            {
                builder.AddComponentParameter(2, nameof(AdvancedTableSelectColumnBodyCellContent<>.Item), item);
                builder.AddComponentParameter(3, nameof(AdvancedTableSelectColumnBodyCellContent<>.Value), AdvancedTable.IsSelected(item));
                builder.AddComponentParameter(4, nameof(AdvancedTableSelectColumnBodyCellContent<>.Enabled), IsCellEnabled(item));
                builder.AddComponentParameter(5, nameof(AdvancedTableSelectColumnBodyCellContent<>.ValueChanged),
                    EventCallback.Factory.Create<bool>(this, value => CellCheckBoxValueChangedAsync(value, item)));
            }
            builder.CloseComponent();
        };

    /// <inheritdoc/>
    private protected override int? GetDefaultWidth()
        => GetMinimumWidth();

    /// <inheritdoc/>
    /// <remarks>
    /// The return value is measured against the select-all header rather than a body cell, the header being the narrower of the
    /// two: a 1.25rem checkbox, 2 x 16px of padding, the 1px column separator, and the 1px the browser pads a
    /// <c>th</c> with on either side — 51.25px at the 13px root font size the design baseline sets, rounded
    /// up. The checkbox is sized in rem and everything around it in px, so no single number holds for every
    /// root size: a host setting a larger one outgrows this.
    /// </remarks>
    private protected override int? GetMinimumWidth()
        => 52;
}
