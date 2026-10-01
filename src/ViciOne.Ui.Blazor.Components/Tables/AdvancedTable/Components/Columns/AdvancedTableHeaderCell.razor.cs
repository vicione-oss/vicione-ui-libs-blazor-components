using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// One column header: its label, the sort indicator and filter button it may carry, the handle it may be
/// resized by, and the gap zones a dragged column can be dropped into.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <remarks>
/// Decides for itself what its column supports — sorting, filtering, resizing — and reports gestures as plain
/// intent, so the table is left with applying them rather than with working out what a click meant.
/// </remarks>
public sealed partial class AdvancedTableHeaderCell<TItem> : ComponentBase
    where TItem : class
{
    // Cascaded rather than taken as a parameter: the context and everything in it are internal types, and a
    // Razor component is always generated public, so they cannot appear on a public parameter. Cascading
    // parameters may be private, which is how the rest of this library hands internal types to a component.
    [CascadingParameter]
    private ColumnHeaderContext<TItem> Context { get; set; } = default!;

    /// <summary>
    /// The style placing this header when its column is pinned, otherwise <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? PinStyle { get; set; }

    /// <summary>
    /// The direction the column is sorted in, or <see langword="null"/> when it is not sorted by.
    /// </summary>
    [Parameter]
    public bool? SortAscending { get; set; }

    /// <summary>
    /// The filter currently applied to the column, if any.
    /// </summary>
    [Parameter]
    public IColumnFilter? Filter { get; set; }

    /// <summary>
    /// Whether this header may start a drag. Suppressed while its own filter panel is open, so an in-panel
    /// text drag cannot start a reorder.
    /// </summary>
    [Parameter]
    public bool DragEnabled { get; set; } = true;

    /// <summary>
    /// Raised when the user asks to sort by this column, by click or by <kbd>Enter</kbd>. The value says
    /// whether the gesture was additive (<kbd>Shift</kbd> held), and the direction asked for is the opposite of
    /// the current one. Raised only for a column that is actually sortable.
    /// </summary>
    [Parameter]
    public EventCallback<SortRequest> SortRequested { get; set; }

    /// <summary>
    /// Raised when the column's filter is applied or cleared.
    /// </summary>
    [Parameter]
    public EventCallback<IColumnFilter?> FilterChanged { get; set; }

    /// <summary>
    /// Raised when the column's filter panel opens or closes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FilterOpenChanged { get; set; }

    private IAdvancedTableColumn<TItem> Column => Context.Column;

    private ColumnReorderInteraction<TItem> Reorder => Context.Reorder;

    private bool Sortable => Column is ISortableColumn { Sortable: true };

    private string ThClass
    {
        get
        {
            List<string> classes = [];

            if (Reorder.GetGapIndicatorClass(Column) is { } gapIndicatorClass)
                classes.Add(gapIndicatorClass);

            if (Column.PinSide != PinSide.None)
                classes.Add("pinned-column");

            return string.Join(" ", classes);
        }
    }

    private string HeaderClass
    {
        get
        {
            if (Sortable)
                return "header-cell sortable";

            return "header-cell";
        }
    }

    private string AriaSort
    {
        get
        {
            if (SortAscending is not { } ascending)
                return "none";

            if (ascending)
                return "ascending";

            return "descending";
        }
    }

    // draggable is an HTML enumerated attribute: it must render the literal string "true"/"false". A bool
    // renders true minimized (draggable="") which the browser reads as "auto" — i.e. not draggable.
    private string DraggableAttribute
    {
        get
        {
            if (DragEnabled)
                return "true";

            return "false";
        }
    }

    // The reorder interaction is driven here rather than reported upwards: the header knows which column it
    // stands for, so there is nothing for the table to work out. What the table still has to do is redraw —
    // the gap zones and the drop indicator sit on every other header — which is what ReorderChanged asks for.
    private Task DragStartAsync()
    {
        Reorder.DragStart(Column);

        return Context.ReorderChanged.InvokeAsync();
    }

    private Task DragEndAsync()
    {
        Reorder.DragEnd();

        return Context.ReorderChanged.InvokeAsync();
    }

    private Task GapDragEnterAsync(bool after)
    {
        Reorder.DragEnter(Column, after);

        return Context.ReorderChanged.InvokeAsync();
    }

    private Task GapDragLeaveAsync(bool after)
    {
        Reorder.DragLeave(Column, after);

        return Context.ReorderChanged.InvokeAsync();
    }

    private Task GapDropAsync(bool after)
    {
        Reorder.Drop(Column, after);

        return Context.ReorderChanged.InvokeAsync();
    }

    private Task HeaderClickAsync(MouseEventArgs args)
        => RequestSortAsync(args.ShiftKey);

    private Task KeyUpAsync(KeyboardEventArgs args)
    {
        if (!args.IsEnter())
            return Task.CompletedTask;

        return RequestSortAsync(args.ShiftKey);
    }

    // A column that cannot be sorted reports nothing, so the table is never asked to sort by one and needs no
    // guard of its own.
    private Task RequestSortAsync(bool additive)
    {
        if (!Sortable)
            return Task.CompletedTask;

        // Toggle if already sorting by this column, else start ascending.
        var ascending = !SortAscending ?? true;

        return SortRequested.InvokeAsync(new SortRequest(ascending, additive));
    }
}
