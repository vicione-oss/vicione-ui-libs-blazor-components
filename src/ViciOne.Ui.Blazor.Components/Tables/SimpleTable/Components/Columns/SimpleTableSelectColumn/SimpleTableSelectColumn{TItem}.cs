using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;

/// <summary>
/// A specialized column for the <see cref="SimpleTable{TItem}"/> that provides row selection functionality.
/// Each cell selects its row. Under <see cref="SelectionMode.Multiple"/> the header checkbox toggles
/// selection of the current Filtered Set — the rows the active filter leaves; rows the filter hides are
/// never touched, and the header tri-state likewise reflects only the Filtered Set. Under
/// <see cref="SelectionMode.Single"/> the header is disabled and a cell click selects that single row,
/// deselecting the previously selected one.
/// When <see cref="AdvancedTableSelectColumn{TItem}.DisplayOnly"/> is <see langword="true"/>, both the cell
/// checkbox and the select-all header render their checked/tri-state but disabled and cannot change the selection.
/// </summary>
/// <remarks>
/// Beware: the header select-all covers the whole Filtered Set independent of the virtualization window,
/// so using it materializes every filtered row into the selection. Pagination works as intended and should
/// be preferred for large amounts of data.
/// </remarks>
public sealed class SimpleTableSelectColumn<TItem> : AdvancedTableSelectColumn<TItem>
    where TItem : class
{
    [CascadingParameter]
    internal ISimpleTable<TItem> SimpleTable { get; set; } = default!;

    /// <summary>
    /// Whether the header renders a "select all" checkbox. Defaults to <see langword="true"/>.
    /// When <see langword="false"/>, the header is empty and per-row selection still works.
    /// </summary>
    [Parameter]
    public bool HasSelectAllHeader { get; set; } = true;

    // Select-all is a Multiple-mode affordance: disabled when the column channel is display-only (DisplayOnly —
    // the tri-state still renders, but the header cannot mutate the selection), under Single (cells act as
    // single-select), when the Filtered Set is empty (nothing to act on), and when a selection constraint
    // makes the required all-at-once order ill-defined.
    private bool IsHeaderEnabled
        => !DisplayOnly
            && AdvancedTable.SelectionMode != SelectionMode.Single
            && SimpleTable.FilteredItems.Count > 0
            && AdvancedTable.ItemSelectionAllowed is null;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        // Guard before base.OnInitialized(): base registers the column with the table, and a registered
        // column's header renders — against the missing SimpleTable — before this exception surfaces.
        if (SimpleTable is null)
            throw new InvalidOperationException($"{GetType().FullName} must be placed inside a {typeof(ISimpleTable<TItem>).FullName}.");

        base.OnInitialized();

        AdvancedTable.ItemsChanged += HandleItemsChanged;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            AdvancedTable.ItemsChanged -= HandleItemsChanged;

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override RenderFragment GetHeader()
    {
        if (HasSelectAllHeader)
        {
            return builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumnHeaderCellContent<TItem>>(1);
                {
                    builder.AddComponentParameter(2, nameof(SimpleTableSelectColumnHeaderCellContent<>.Enabled), IsHeaderEnabled);
                    builder.AddComponentParameter(3, nameof(SimpleTableSelectColumnHeaderCellContent<>.Value), GetHeaderState());
                    builder.AddComponentParameter(4, nameof(SimpleTableSelectColumnHeaderCellContent<>.ValueChanged),
                        EventCallback.Factory.Create<bool?>(this, HeaderCheckBoxValueChangedAsync));
                }
                builder.CloseComponent();
            };
        }

        return base.GetHeader();
    }

    private void HandleItemsChanged()
        => RequestRefresh();

    private bool? GetHeaderState()
    {
        // The tri-state is scoped to the Filtered Set: it answers "is everything the filter leaves selected?"
        // and ignores hidden selections entirely, so a select-all always lands on checked — never an
        // unexplained dash caused by rows the filter hides.
        var filteredItems = SimpleTable.FilteredItems;
        var selectedCount = AdvancedTable.CountSelected(filteredItems);

        // cases: nothing in the Filtered Set selected (false), all of it selected (true), otherwise (null)
        bool? value = null;

        if (selectedCount == 0)
            value = false;
        else if (selectedCount == filteredItems.Count)
            value = true;

        return value;
    }

    private async Task HeaderCheckBoxValueChangedAsync(bool? selectAll)
    {
        // Display-only column channel: the select-all header self-gates here so a disabled header that still manages
        // to raise a change (e.g. a forced event) cannot mutate the selection. Mirrors the cell's DisplayOnly guard.
        if (DisplayOnly || selectAll is null)
            return;

        // Bulk selection is scoped to the Filtered Set: it acts on the rows the active filter leaves, never
        // on rows the filter hides — so select-all cannot silently reach rows the user cannot see, and
        // deselect-all leaves hidden selections intact.
        var filteredItems = SimpleTable.FilteredItems;

        if (selectAll is true)
            await AdvancedTable.SelectAsync(filteredItems);
        else
            await AdvancedTable.DeselectAsync(filteredItems);
    }
}
