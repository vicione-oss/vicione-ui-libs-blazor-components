using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// Base class providing core functionality for the column components of <see cref="AdvancedTable{TItem}"/>.
/// </summary>
/// <remarks>
/// Deriving from this outside the package is not supported: the members a column must supply are
/// <see langword="private protected"/>, and the capabilities the table branches on — sorting, resizing,
/// pinning, filtering — are internal. Render custom content through
/// <see cref="AdvancedTableColumn{TItem}.HeaderContent"/> and
/// <see cref="AdvancedTableColumn{TItem}.CellContent"/> instead.
/// </remarks>
public abstract partial class AdvancedTableColumnBase<TItem> : ComponentBase, IAdvancedTableColumn<TItem>, IDisposable
    where TItem : class
{
    private ColumnState? _columnState;

    /// <summary>
    /// Pins the column to the left or right edge so it stays visible while the unpinned columns scroll
    /// horizontally. Pinned columns render grouped by side — all left pins, then the unpinned columns, then
    /// all right pins — regardless of declaration order, and a column can only be dragged within its group.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="PinSide.None"/>.
    /// </remarks>
    [Parameter]
    public PinSide PinSide { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// Defaults to <see langword="true"/>
    /// </remarks>
    [Parameter]
    public bool ShowInColumnChooser { get; set; } = true;

    /// <summary>
    /// Whether the column is visible in the table. Hiding a column also drops the filter and the sorting
    /// keyed to it, because a hidden column has no header left to display or clear them from.
    /// <para>
    /// The value supplied from markup seeds the visibility and the <see cref="ColumnChooserContent{TItem}"/> writes back to
    /// it, so a hard-coded value is re-applied — and a chooser-driven change to it lost — whenever the parent
    /// re-renders. Bind with <c>@bind-Visible</c> to keep both sides in step.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>
    /// </remarks>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Raised after <see cref="Visible"/> changed, whether through the <see cref="ColumnChooserContent{TItem}"/> or through
    /// a markup-driven change of the parameter. Supports two-way binding with <c>@bind-Visible</c>.
    /// </summary>
    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// Raised when a body cell of this column is activated, with the item the cell's row shows. A cell is
    /// activated by Enter while it has the focus.
    /// <para>
    /// Entering the cell follows the activation: the focus moves into the cell's first focusable content, once
    /// the render the handler caused has been applied, so an editor the handler swaps into the cell receives the
    /// focus. Clicking into a cell enters it without activating it. Left unset, nothing is raised, and Enter
    /// still moves the focus into whatever the cell already holds. A cell with no focusable content keeps
    /// the focus. This cannot be canceled: to keep the keyboard out of a cell, render nothing focusable in it,
    /// or move the focus from the handler.
    /// </para>
    /// <para>
    /// Scoped to the column rather than to the table, so the handler already knows which cell was activated
    /// and needs no column argument to branch on. The flip side is that a column left unwired raises nothing:
    /// a table where Enter should act everywhere wires every column.
    /// </para>
    /// </summary>
    [Parameter]
    public EventCallback<TItem> CellActivated { get; set; }

    /// <summary>
    /// Reference to the parent table.
    /// </summary>
    [CascadingParameter]
    internal IAdvancedTable<TItem> AdvancedTable { get; set; } = default!;

    [Inject]
    private ILogger<AdvancedTableColumnBase<TItem>> Logger { get; set; } = default!;

    /// <inheritdoc/>
    string IAdvancedTableColumn<TItem>.Id => GetColumnId();

    /// <inheritdoc/>
    RenderFragment IAdvancedTableColumn<TItem>.Header => GetHeader();

    /// <inheritdoc/>
    RenderFragment<TItem> IAdvancedTableColumn<TItem>.CellContent => GetCellContent();

    /// <inheritdoc/>
    /// <remarks>
    /// Never narrower than the column's own minimum. Clamped here rather than in each column, so a column
    /// cannot hand out a width that undercuts the minimum it has just declared. A flex column declares no
    /// width at all and keeps its <see langword="null"/>: the minimum floors it in the browser instead, once
    /// there is a container to share out.
    /// </remarks>
    int? IAdvancedTableColumn<TItem>.DefaultWidth
    {
        get
        {
            if (GetDefaultWidth() is not { } width)
                return null;

            return GetMinimumWidth() is { } minimumWidth ? Math.Max(width, minimumWidth) : width;
        }
    }

    /// <inheritdoc/>
    ColumnState? IAdvancedTableColumn<TItem>.State => _columnState;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (AdvancedTable is null)
            throw new InvalidOperationException($"{GetType().FullName} must be placed inside a {typeof(AdvancedTable<TItem>).FullName}.");

        AdvancedTable.RegisterColumn(this);
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_columnState is null)
            return;

        _columnState.Visible = Visible;
        _columnState.PinSide = PinSide;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs synchronous clean-up
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            AdvancedTable.UnregisterColumn(this);
    }

    /// <summary>
    /// Gets the unique identifier for the column.
    /// </summary>
    private protected abstract string GetColumnId();

    /// <summary>
    /// Gets the header for the column.
    /// </summary>
    private protected abstract RenderFragment GetHeader();

    /// <summary>
    /// Gets the content rendered into each of the column's body cells. A column that shows nothing returns an
    /// empty fragment: the cell is still emitted, so colgroup and thead stay aligned with the later columns.
    /// </summary>
    private protected abstract RenderFragment<TItem> GetCellContent();

    /// <summary>
    /// Gets the default width of the column in pixels before any user resize.
    /// When not set, the column shares remaining space equally with other columns without a default width.
    /// </summary>
    private protected abstract int? GetDefaultWidth();

    /// <summary>
    /// Gets the narrowest the column may be drawn, in pixels. A column whose cell holds something that cannot
    /// be read cut in half — a checkbox rather than a word — states the room that thing needs.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> by default, meaning the column names no minimum at all: one showing text has
    /// nothing it must keep whole, and cutting a long value short is what the cell's ellipsis is for.
    /// </remarks>
    private protected virtual int? GetMinimumWidth()
        => null;

    // Reaches the state the table paired with this column. A derived column holding a reference to its own
    // rendered header instead would break as soon as that header renders twice — the column chooser renders
    // the same fragment as its label — because the second render replaces the reference the first captured.
    private protected void RequestRefresh()
        => _columnState?.RequestRefresh();

    /// <inheritdoc/>
    void IAdvancedTableColumn<TItem>.SetState(ColumnState? columnState)
    {
        if (columnState != _columnState)
        {
            _columnState?.VisibleChanged -= ColumnStateVisibleChangedAsync;

            _columnState = columnState;

            // Without this seed the first OnParametersSet raises a pin side change the column never made,
            // and the table answers that with a render.
            _columnState?.PinSide = PinSide;

            _columnState?.VisibleChanged += ColumnStateVisibleChangedAsync;
        }
    }

    private async void ColumnStateVisibleChangedAsync(ColumnState columnState)
    {
        try
        {
            await UpdateVisibleAsync(columnState.Visible);
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // Disposal left the column subscribed and the renderer is gone, nothing we can do
        }
        catch (Exception exception)
        {
            // Nothing above this can observe a failure — an escaping exception would tear the circuit down
            // instead of surfacing. The table already renders the new visibility; only the consumer's bound
            // property was not updated.
            VisibleChangedCallbackFailed(Logger, exception, GetType().Name);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{Component} failed to report a column visibility change: " +
        "the supplied VisibleChanged callback threw. The column renders at its new visibility, but the " +
        "consumer's bound property still holds the old one.")]
    private static partial void VisibleChangedCallbackFailed(ILogger logger, Exception ex, string component);

    private async Task UpdateVisibleAsync(bool value)
    {
        if (Visible != value)
        {
            Visible = value;

            if (VisibleChanged.HasDelegate)
                await VisibleChanged.InvokeAsync(Visible);
        }
    }
}
