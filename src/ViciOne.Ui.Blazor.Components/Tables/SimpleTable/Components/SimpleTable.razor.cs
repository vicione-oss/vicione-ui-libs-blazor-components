using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;

/// <summary>
/// Component for rendering a table
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public sealed partial class SimpleTable<TItem> : ComponentBase, ISimpleTable<TItem>, IDisposable
    where TItem : class
{
    private AdvancedTable<TItem>? _advancedTable;

    // Null until the first parameter set builds it, and rebuilt whenever the item source reference changes.
    // A fresh provider starts with an empty Filtered Set, so the select-all header renders disabled for the
    // one frame between the rebuild and the re-provide that follows it.
    private SimpleTableItemsProvider<TItem>? _itemProvider;

    private IReadOnlyList<TItem>? _previousItems;

    private List<TItem>? _previousSelectedItems;

    private bool _awaitingItems;

    private bool _raiseFilteredItems;

    private bool _raiseVisibleSelection;

    private IAdvancedTable<TItem>? _subscribedTable;

    // OnParametersSet builds the provider before the first render, so this only ever creates one if the
    // markup is reached first; it exists so the markup needs no null-forgiving operator.
    private SimpleTableItemsProvider<TItem> ItemProvider => _itemProvider ??= CreateItemProvider();

    // SimpleTable keeps no filter or sorting state of its own — the wrapped AdvancedTable owns it. A second
    // copy here could only ever disagree with the first.

    [Inject]
    private ILogger<SimpleTableItemsProvider<TItem>> ItemsProviderLogger { get; set; } = default!;

    [Inject]
    private ISimpleTableSortComparerResolver<TItem> SortComparerResolver { get; set; } = default!;

    /// <summary>
    /// Items to be displayed in the table.
    ///</summary>
    /// <remarks>
    /// The component filters and sorts them in-memory,
    /// so this is a materialized list (pass <c>.ToList()</c> for a lazy source);
    /// it no longer promises data-source pushdown.
    /// </remarks>
    [Parameter, EditorRequired]
    public required IReadOnlyList<TItem> Items { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.Header"/>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.Columns"/>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.Footer"/>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.LeftOutlet"/>
    [Parameter]
    public RenderFragment? LeftOutlet { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.ColumnChooserToggleId"/>
    [Parameter]
    public object? ColumnChooserToggleId { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.Mode" path="/summary"/>
    /// <remarks>
    /// Needs no <c>ItemIdSelector</c>: the built-in provider returns the same instances on every fetch, so
    /// selection is matched by reference without one.
    /// </remarks>
    [Parameter]
    public TableLoadingMode Mode { get; set; } = TableLoadingMode.All;

    /// <inheritdoc cref="AdvancedTable{TItem}.CssClass"/>
    [Parameter]
    public string? CssClass { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.MaximumHeight"/>
    [Parameter]
    public string? MaximumHeight { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.Striped"/>
    [Parameter]
    public bool Striped { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.SortingState"/>
    [Parameter]
    public SortingState SortingState { get; set; } = SortingState.Empty;

    /// <inheritdoc cref="AdvancedTable{TItem}.SortingStateChanged"/>
    [Parameter]
    public EventCallback<SortingState> SortingStateChanged { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.FilterState"/>
    [Parameter]
    public FilterState FilterState { get; set; } = FilterState.Empty;

    /// <inheritdoc cref="AdvancedTable{TItem}.FilterStateChanged"/>
    [Parameter]
    public EventCallback<FilterState> FilterStateChanged { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.SelectionMode"/>
    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.GetDefaultValue();

    /// <inheritdoc cref="AdvancedTable{TItem}.RowClickSelectionEnabled"/>
    [Parameter]
    public bool RowClickSelectionEnabled { get; set; } = true;

    /// <inheritdoc cref="AdvancedTable{TItem}.SelectedItems"/>
#pragma warning disable CA2227 // Collection properties should be read only — Blazor parameters require a setter for data binding
    [Parameter]
    public List<TItem> SelectedItems { get; set; } = [];
#pragma warning restore CA2227

    /// <inheritdoc cref="AdvancedTable{TItem}.SelectedItemsChanged"/>
    [Parameter]
    public EventCallback<List<TItem>> SelectedItemsChanged { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.ItemSelectionAllowed"/>
    [Parameter]
    public Func<SelectionRequest<TItem>, bool>? ItemSelectionAllowed { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.DragPayloadProvider"/>
    [Parameter]
    public ITableDragPayloadProvider<TItem>? DragPayloadProvider { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.RowsDraggable"/>
    [Parameter]
    public bool RowsDraggable { get; set; }

    /// <inheritdoc cref="AdvancedTable{TItem}.RowDoubleClick"/>
    [Parameter]
    public EventCallback<TItem> RowDoubleClick { get; set; }

    /// <summary>
    /// An optional callback invoked when a row is right-clicked, receiving a
    /// <see cref="RowContextMenuEventArgs{TItem}"/> with the row's data item and mouse position. The
    /// table reports the right-click only; to open a menu the consumer builds its own
    /// <c>IContextMenuContext</c> in the handler and sends it. The browser's native context menu is
    /// suppressed while a handler is wired and <c>IContextMenuSettings.UseCustomMenu</c> is
    /// <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The setting governs suppression only: the callback is invoked on every row right-click, whether
    /// or not <c>UseCustomMenu</c> is <see langword="true"/>. A handler that does more than send a
    /// context-menu request — moving the selection, say — has to check the setting itself, or those
    /// effects land while the browser is opening its own menu.
    /// </remarks>
    [Parameter]
    public EventCallback<RowContextMenuEventArgs<TItem>> RowContextMenuRequested { get; set; }

    /// <summary>
    /// An optional callback invoked with the rows the active filter leaves, whenever the filter or
    /// <see cref="Items"/> changes. This is the set a bulk selection should select <em>from</em>.
    /// </summary>
    /// <remarks>
    /// <para>Not a bindable parameter: the table computes the value and there is nothing to set. Raised once per
    /// change of the filter or the item source, never per rendered window, so scrolling does not raise it.</para>
    /// <para>Independent of virtualization — the whole filtered set, not the loaded window — and in the order
    /// <see cref="Items"/> holds the rows, which is not the order the table displays them in.</para>
    /// <para>For the visible selection use <see cref="VisibleSelectionChanged"/> rather than intersecting this
    /// with the selection by hand.</para>
    /// <para>When no filter is active the list handed over is the <see cref="Items"/> instance itself, so treat
    /// it as read-only and do not take its reference as a change signal.</para>
    /// </remarks>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> FilteredItemsChanged { get; set; }

    /// <summary>
    /// An optional callback invoked with the selected rows that the active filter leaves — the visible
    /// selection, which is what an action on the selection should operate on.
    /// </summary>
    /// <remarks>
    /// <para>Not a bindable parameter: the table computes the value and there is nothing to set. Raised whenever
    /// the selection changes — by the table or by a <see cref="SelectedItems"/> reference replaced from outside —
    /// and whenever the filter or the item source changes. Scrolling does not raise it.</para>
    /// <para>Independent of virtualization: a selected row that passes the filter but has scrolled out of the
    /// loaded window is still included. Ordered as <see cref="Items"/> holds the rows, which is neither the order
    /// the table displays them in nor the order they were selected in.</para>
    /// <para>Filtering never removes anything from <see cref="SelectedItems"/>; a row hidden by the filter stays
    /// selected and falls out of this list until the filter no longer hides it.</para>
    /// </remarks>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> VisibleSelectionChanged { get; set; }

    /// <inheritdoc/>
    IReadOnlyList<TItem> ISimpleTable<TItem>.Items => Items;

    /// <inheritdoc/>
    IReadOnlyList<TItem> ISimpleTable<TItem>.FilteredItems => _itemProvider?.FilteredItems ?? [];

    // Both methods below no-op while the wrapped table is unset, which a consumer cannot observe: an @ref to
    // this component is only populated after the render that creates it. Both dispatch before reading that
    // field, so a caller on another thread cannot see it half-assigned and silently do nothing.

    /// <summary>
    /// Applies <paramref name="filterState"/> and re-fetches, immediately and regardless of what the
    /// <see cref="FilterState"/> parameter has already delivered. Entries for columns the table does not show
    /// are dropped; a value that leaves the applied filter state unchanged does nothing at all.
    /// </summary>
    /// <param name="filterState">The filter state to apply. Use <see cref="FilterState.Empty"/> to clear.</param>
    /// <remarks>
    /// <para>The peer of the <see cref="FilterState"/> parameter, for the changes a binding expresses badly — a
    /// "clear all" button, a reset to a preset — and for callers that are not in a render at all. Unlike the
    /// parameter it has no memory of what it was handed before, so a value the parameter channel has already
    /// seen can still be pushed through here.</para>
    /// <para>Safe to call from any thread: a filter driven from outside can just as well come from a debounce
    /// timer or a server push as from a button, and neither of those runs where the renderer expects its state
    /// to be touched.</para>
    /// </remarks>
    public Task SetFilterStateAsync(FilterState filterState)
        => InvokeAsync(() => _advancedTable?.SetFilterStateAsync(filterState) ?? Task.CompletedTask);

    /// <summary>
    /// Applies <paramref name="sortingState"/> and re-fetches, immediately and regardless of what the
    /// <see cref="SortingState"/> parameter has already delivered. Entries for columns the table does not show
    /// are dropped; a value that leaves the applied sorting state unchanged does nothing at all.
    /// </summary>
    /// <param name="sortingState">The sorting state to apply. Use <see cref="SortingState.Empty"/> to clear.</param>
    /// <remarks>
    /// The sorting peer of <see cref="SetFilterStateAsync"/>; see there for when to reach for the method rather
    /// than the parameter, and for the threading guarantee.
    /// </remarks>
    public Task SetSortingStateAsync(SortingState sortingState)
        => InvokeAsync(() => _advancedTable?.SetSortingStateAsync(sortingState) ?? Task.CompletedTask);

    private async Task AdvancedTableSelectedItemsChangedAsync(List<TItem> items)
    {
        SelectedItems = items;

        _previousSelectedItems = items;

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(items);

        if (_awaitingItems)
        {
            _raiseVisibleSelection = true;

            return;
        }

        if (VisibleSelectionChanged.HasDelegate)
            await VisibleSelectionChanged.InvokeAsync(GetVisibleSelection());
    }

    private async Task AdvancedTableFilterStateChangedAsync(FilterState filterState)
    {
        _awaitingItems = true;

        if (FilterStateChanged.HasDelegate)
            await FilterStateChanged.InvokeAsync(filterState);
    }

    private async Task AdvancedTableSortingStateChangedAsync(SortingState sortingState)
    {
        if (SortingStateChanged.HasDelegate)
            await SortingStateChanged.InvokeAsync(sortingState);
    }

    private void AdvancedTableItemsChanged()
    {
        if (!Interlocked.CompareExchange(ref _awaitingItems, false, true))
            return;

        _raiseFilteredItems = true;
        _raiseVisibleSelection = true;

        // The callbacks have to be awaited and this notification is synchronous, so the render hands the work to
        // OnAfterRenderAsync, where the framework observes the task.
        StateHasChanged();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        // Recreate the built-in provider only when the item source reference changes; the wrapped
        // AdvancedTable re-provides through it. Wrong-type filters are logged and skipped by the provider,
        // not rejected here — SimpleTable keeps no filter state.
        if (_previousItems != Items)
        {
            _itemProvider = CreateItemProvider();
            _previousItems = Items;

            _awaitingItems = true;
        }

        if (_previousSelectedItems != SelectedItems)
        {
            _previousSelectedItems = SelectedItems;

            _raiseVisibleSelection = true;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // @ref populates the field only once a render has produced the wrapped table.
        if (_subscribedTable is null && _advancedTable is not null)
        {
            _subscribedTable = _advancedTable;
            _subscribedTable.ItemsChanged += AdvancedTableItemsChanged;
        }

        if (_awaitingItems)
            return;

        if (!_raiseFilteredItems && !_raiseVisibleSelection)
            return;

        var raiseFilteredItems = _raiseFilteredItems;
        var raiseVisibleSelection = _raiseVisibleSelection;

        // A consumer that reacts by setting state re-renders this component, and a flag still standing at that
        // point would report the same values a second time.
        _raiseFilteredItems = false;
        _raiseVisibleSelection = false;

        if (raiseFilteredItems && FilteredItemsChanged.HasDelegate)
            await FilteredItemsChanged.InvokeAsync(_itemProvider?.FilteredItems ?? []);

        if (raiseVisibleSelection && VisibleSelectionChanged.HasDelegate)
            await VisibleSelectionChanged.InvokeAsync(GetVisibleSelection());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_subscribedTable is null)
            return;

        _subscribedTable.ItemsChanged -= AdvancedTableItemsChanged;
        _subscribedTable = null;
    }

    private IReadOnlyList<TItem> GetVisibleSelection()
    {
        var filteredItems = _itemProvider?.FilteredItems ?? [];

        if (filteredItems.Count == 0 || SelectedItems.Count == 0)
            return [];

        HashSet<TItem> selectedItems = [.. SelectedItems];

        return [.. filteredItems.Where(selectedItems.Contains)];
    }

    private SimpleTableItemsProvider<TItem> CreateItemProvider()
        => new(Items, () => ((IAdvancedTable<TItem>?)_advancedTable)?.Columns ?? [], ItemsProviderLogger, SortComparerResolver);
}
