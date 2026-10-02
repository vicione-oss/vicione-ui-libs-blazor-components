using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Comparers;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;

/// <summary>
/// Component for rendering a table
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public sealed partial class AdvancedTable<TItem>
    : ComponentBase, IAdvancedTable<TItem>, ITableDragPayloadResolver<TItem>, IAsyncDisposable
        where TItem : class
{
    // Beware: Manual handling of these is required to prevent breaking the refs.
    // Blazor recreates on rerender but existing cells still have a ref to the previous one, potentially causing issues.
    private readonly Dictionary<object, RowState> _rowStates = [];

    // Resolves a rendered cell back to the item its row shows. A row is named by the sequence number its
    // cells carry, never by its position: a position read out of the DOM resolves to a different item as
    // soon as the loaded window has moved since that cell was rendered, which under Interactive Server is
    // one round trip away. Sequence numbers are handed out once and never reused, so a cell left over from
    // an earlier pass resolves to nothing at all rather than to whichever item now occupies its place.
    private readonly Dictionary<int, TItem> _itemsByRowSequence = [];

    private int _nextRowSequence;

    // Where each loaded item sits in the filtered, sorted set. Rebuilt whenever the loaded window is
    // replaced rather than counted while rendering: Virtualize re-renders its rows without the table
    // rendering, so a counter held in the table's render block would keep climbing instead of restarting.
    private Dictionary<TItem, int> _absoluteRowIndexes = [];

    // Which columns the table has, the order it draws them in, and how wide each one is.
    private readonly ColumnLayout<TItem> _columnLayout = new();

    // Drag-reordering of the column headers, which moves columns within _columnLayout. Created on first use
    // because a field initializer cannot reach the layout it is built on.
    private ColumnReorderInteraction<TItem>? _columnReorderInteraction;

    // Keyed by column instance so states stay paired with their column across unregister and reorder;
    // a positional pairing desyncs as soon as a column is removed from the middle, and a column-id key
    // goes stale when a column's Id parameter is changed after registration.
    private readonly Dictionary<IAdvancedTableColumn<TItem>, ColumnState> _columnStates = [];

    // Held rather than rebuilt per render so each header keeps the same context instance for as long as its
    // column exists. The cascade is fixed, which promises the value never changes; a fresh record every pass
    // would quietly break that promise.
    private readonly Dictionary<IAdvancedTableColumn<TItem>, ColumnHeaderContext<TItem>> _columnHeaderContexts = [];

    private ElementReference _tableElement;

    // Created on first use rather than in a field initializer: the IJSRuntime it needs is injected after the
    // component is constructed.
    private AdvancedTableJsSession<AdvancedTable<TItem>>? _jsSession;


    private Virtualize<TItem>? _virtualizeComponent;

    private bool _ignoreNextRender;
    private RowState? _previousHoveredRowState;

    // The rows the table holds and the provider calls that fill them. Created on first use because the logger
    // it reports through is injected after the component is constructed.
    private TableItemSource<TItem>? _tableItemSource;

    private bool _disposed;
    private bool _pendingColumnsChanged;
    private HashSet<string> _sizedColumnIds = [];

    private bool _cellEntryPending;


    private IItemsProvider<TItem>? _previousItemsProvider;
    private TableLoadingMode _previousMode;

    // The applied filter and sorting state, owned solely by the table. A parent may hold a copy through the
    // binding, but the table never depends on it being fed back. Everything reads these — the provider
    // context, the active filter icon, aria-sort, the sort indicator.
    private FilterState _appliedFilterState = FilterState.Empty;
    private SortingState _appliedSortingState = SortingState.Empty;

    // The last instance each parameter delivered, whether or not it was applied. Written on every parameter
    // pass: recording it only on apply would let a parent that binds but ignores the change event revert the
    // user, because its stale value would look like a new command on the next unrelated render.
    private FilterState? _previousFilterState;
    private SortingState? _previousSortingState;

    // Raised when a parameter delivered a command; the value is re-read at apply time, so several parameter
    // passes before one render collapse to the latest.
    private bool _pendingFilterState;
    private bool _pendingSortingState;

    // Column-scoped state can only be settled once the columns exist, so it is applied after a render. This
    // gates everything that must not run before that first pass.
    private bool _columnDependentStateApplied;

    private readonly HashSet<string> _loggedDroppedFilterColumnIds = [];
    private readonly HashSet<string> _loggedDroppedSortingColumnIds = [];

    private string? _openFilterColumnId;

    private TItem? _selectionAnchor;
    private int? _selectionAnchorIndex;

    private List<TItem> _selectedItems = [];

    // Membership for the per-row checkboxes, which ask once per row per render. Rebuilt from _selectedItems
    // rather than maintained, because the selection is replaced wholesale on every commit.
    private HashSet<TItem>? _selectedItemSet;
    private List<TItem>? _previousSelectedItems;

    private Func<TItem, object>? _previousItemIdSelector;
    private IEqualityComparer<TItem> _selectionComparer = ItemIdComparer.For<TItem>(itemIdSelector: null);

    private bool _loggedSelectionMisconfiguration;

    IReadOnlyList<TItem> IAdvancedTable<TItem>.SelectedItems => _selectedItems;

    IReadOnlyList<IAdvancedTableColumn<TItem>> IAdvancedTable<TItem>.Columns => _columnLayout.Columns;

    Func<SelectionRequest<TItem>, bool>? IAdvancedTable<TItem>.ItemSelectionAllowed => ItemSelectionAllowed;

    internal IReadOnlyCollection<TItem> InputItems => ItemSource.Items;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    // The table's entire JavaScript surface. Also cascaded to the column filter panel, which positions itself
    // through the same module.
    private AdvancedTableJsSession<AdvancedTable<TItem>> JsSession => _jsSession ??= new(JsRuntime, this);

    private ColumnReorderInteraction<TItem> ColumnReorder => _columnReorderInteraction ??= new(_columnLayout);

    private TableItemSource<TItem> ItemSource => _tableItemSource ??= new(Logger, nameof(AdvancedTable<>));

    [Inject]
    private ILogger<AdvancedTable<TItem>> Logger { get; set; } = default!;

    // Registered by AddAdvancedTable so a table that never opens a menu still resolves it.
    [Inject]
    private IContextMenuSettings ContextMenuSettings { get; set; } = default!;

    // The setting raises no change notification, so it is read on render and a UseCustomMenu toggle
    // reaches the rows on the table's next render.
    private bool NativeContextMenuSuppressed
        => RowContextMenuRequested.HasDelegate && ContextMenuSettings.UseCustomMenu;

    /// <summary>
    /// A callback that provides data to the table asynchronously.
    /// This function is invoked by the table whenever it needs to fetch new items,
    /// such as during the initial load, pagination, sorting, or filtering events.
    /// It receives an <see cref="ItemsProviderContext"/> containing the current table state
    /// and must return a <see cref="ItemsProviderResult{TItem}"/> containing the requested items and the total
    /// item count.
    /// </summary>
    [Parameter, EditorRequired]
    public required IItemsProvider<TItem> ItemsProvider { get; set; }

    /// <summary>
    /// Content for the table header area. Left unset, no header area is rendered.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Definition of the table columns in the form of <see cref="AdvancedTableColumnBase{TItem}"/> components.
    /// </summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// Content for the table footer area. Left unset, no footer area is rendered.
    /// </summary>
    /// <remarks>
    /// There is no footer by default. To show the item count, use component
    /// <see cref="TableFooter{TItem}"/> and supply <see cref="TableFooter{TItem}.SelectedItemCount"/>
    /// to show a selection count beside it.
    /// </remarks>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Content for the left outlet. Left unset, no left outlet is rendered.
    /// </summary>
    [Parameter]
    public RenderFragment? LeftOutlet { get; set; }

    /// <summary>
    /// Unique identifier of an optional <see cref="ColumnChooserToggle"/>,
    /// omit to not offer a column chooser toggle and therefore no column chooser for this table.
    /// </summary>
    /// <remarks>
    /// It is used internally to bridge the gap between toggle and table component,
    /// hence it must match with the <see cref="ColumnChooserToggle.Id"/>.
    /// </remarks>
    [Parameter]
    public object? ColumnChooserToggleId { get; set; }

    /// <summary>
    /// Determines how loading of data is handled.
    ///
    /// <para>
    /// Defaults to <see cref="TableLoadingMode.All"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Only use <see cref="TableLoadingMode.All"/> for small amounts of data.
    ///
    /// <para>
    /// When using <see cref="TableLoadingMode.Virtualize"/>, also ensure the table's height is limited, by
    /// <see cref="MaximumHeight"/>, by a rule for <see cref="CssClass"/>, or by the parent layout. Otherwise
    /// every row is laid out and virtualization has no effect.
    /// </para>
    ///
    /// <para>
    /// Both modes support Shift-click ranges. Under <see cref="TableLoadingMode.Virtualize"/> a range reaching
    /// outside the loaded rows costs one extra provider call and requires an <see cref="ItemIdSelector"/>.
    /// </para>
    /// </remarks>
    [Parameter]
    public TableLoadingMode Mode { get; set; } = TableLoadingMode.All;

    /// <summary>
    /// Text rendered into the <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the table's root HTML element.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// The maximum height of the table container, above which the table scrolls internally.
    /// </summary>
    /// <remarks>
    /// Can be specified in different CSS units, e.g., <c>300px</c> or <c>30%</c>. A percentage only limits the
    /// table when its parent has a definite height; within a parent sized by its content it has no effect.
    ///
    /// <para>
    /// Takes precedence over a <c>max-height</c> set by a rule for <see cref="CssClass"/>.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? MaximumHeight { get; set; }

    /// <summary>
    /// An optional callback invoked when a row is double-clicked, receiving the row's data item.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> RowDoubleClick { get; set; }

    /// <summary>
    /// When <see langword="true"/>, every other row has a different color. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// When <see langword="true"/>, rows can be dragged. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool RowsDraggable { get; set; }

    /// <summary>
    /// An optional callback invoked when a row is right-clicked, receiving a
    /// <see cref="RowContextMenuEventArgs{TItem}"/> with the row's data item and mouse position. When
    /// unwired, nothing is invoked and the browser's native context menu is left untouched.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The table reports the right-click and nothing more — it builds no context-menu context, holds no
    /// request handle, and applies no item filtering. To open a menu the consumer builds its own
    /// <c>IContextMenuContext</c> in the handler and sends it, exactly as every other context-menu
    /// consumer does. That is what lets the consumer decide what the menu is about: which rows it acts
    /// on, and which entries apply to them.
    /// </para>
    /// <para>
    /// The native menu is suppressed only while a handler is wired <em>and</em>
    /// <c>IContextMenuSettings.UseCustomMenu</c> is <see langword="true"/>, so switching custom menus
    /// off hands the browser's menu back on rows as it does everywhere else. The setting is read on
    /// render and raises no change notification, so a toggle takes effect on the table's next render.
    /// </para>
    /// <para>
    /// Suppression is the only thing the setting governs here: the callback is invoked on every row
    /// right-click, whether or not <c>UseCustomMenu</c> is <see langword="true"/>. It reports the
    /// right-click, it does not request a menu. A handler that does more than send a context-menu
    /// request — moving the selection, say — has to check the setting itself, or those effects land
    /// while the browser is opening its own menu.
    /// </para>
    /// <para>
    /// Suppression covers the whole row, cell content included. Cell content that needs the native menu
    /// — a text input, say — opts out with <c>@oncontextmenu:stopPropagation="true"</c>.
    /// </para>
    /// </remarks>
    [Parameter]
    public EventCallback<RowContextMenuEventArgs<TItem>> RowContextMenuRequested { get; set; }

    /// <summary>
    /// The sorting to apply — the columns to sort by and their directions. Supports two-way binding through
    /// <see cref="SortingStateChanged"/>. Defaults to <see cref="SortingState.Empty"/>, so an unset parameter
    /// means "no sorting".
    /// </summary>
    /// <remarks>
    /// <para>Applied whenever its instance changes, for the whole lifetime of the table. Handing over the same
    /// instance again is no command, so a value that never changes behaves as the sorting the table starts
    /// with; handing over the table's own value again — what a binding does on every render — is no command
    /// either.</para>
    /// <para>Because a new instance is what makes a command, a consumer that builds a content-equal but
    /// distinct value on every render has it applied on every render. Combined with a binding that never
    /// terminates: each apply raises the event, each raise renders the parent, each render builds another
    /// instance. Hold the value in a field and only replace it when it really changes.</para>
    /// <para>An entry for a column the table does not show is dropped — including for a column that only
    /// registers later, since "not here yet" and "never coming" are indistinguishable. The table applies and
    /// reports the reduced value, which under a binding overwrites the consumer's own field; a persisted view
    /// therefore loses that column's sorting. A column arriving late needs the value handed over again.</para>
    /// <para>Clicking a header sorts by that column alone; Shift-clicking adds it to the existing sorting.
    /// Enter and Shift+Enter on a focused header do the same.</para>
    /// </remarks>
    [Parameter]
    public SortingState SortingState { get; set; } = SortingState.Empty;

    /// <summary>
    /// An event callback that is invoked whenever the applied sorting state changes, whatever caused it — a
    /// header click, <see cref="SetSortingStateAsync"/>, an applied <see cref="SortingState"/> or a dropped
    /// entry. Enables two-way binding of <see cref="SortingState"/>.
    /// </summary>
    [Parameter]
    public EventCallback<SortingState> SortingStateChanged { get; set; }

    /// <summary>
    /// The filter to apply. Supports two-way binding through <see cref="FilterStateChanged"/>. Defaults to
    /// <see cref="FilterState.Empty"/>, so an unset parameter means "no filter".
    /// </summary>
    /// <remarks>
    /// <para>Applied whenever its instance changes, for the whole lifetime of the table. Handing over the same
    /// instance again is no command, so a value that never changes behaves as the filter the table starts
    /// with; handing over the table's own value again — what a binding does on every render — is no command
    /// either.</para>
    /// <para>Because a new instance is what makes a command, a consumer that builds a content-equal but
    /// distinct value on every render has it applied on every render. Combined with a binding that never
    /// terminates: each apply raises the event, each raise renders the parent, each render builds another
    /// instance. Hold the value in a field and only replace it when it really changes.</para>
    /// <para>An entry for a column the table does not show is dropped — including for a column that only
    /// registers later, since "not here yet" and "never coming" are indistinguishable. The table applies and
    /// reports the reduced value, which under a binding overwrites the consumer's own field; a persisted view
    /// therefore loses that column's filter. A column arriving late needs the value handed over again. Global
    /// filters carry no column id and are never dropped.</para>
    /// </remarks>
    [Parameter]
    public FilterState FilterState { get; set; } = FilterState.Empty;

    /// <summary>
    /// An event callback that is invoked whenever the applied filter state changes, whatever caused it — a
    /// filter applied in a column header, <see cref="SetFilterStateAsync"/>, an applied
    /// <see cref="FilterState"/> or a dropped entry. Enables two-way binding of <see cref="FilterState"/>.
    /// </summary>
    [Parameter]
    public EventCallback<FilterState> FilterStateChanged { get; set; }

    /// <summary>
    /// Specifies the selection cardinality across both selection channels (row click and select column):
    /// <see cref="SelectionMode.Single"/> or <see cref="SelectionMode.Multiple"/>. Defaults to
    /// <see cref="SelectionMode.Multiple"/>. Cardinality only — turning a channel off or making it display-only
    /// is done on the channel itself (<see cref="RowClickSelectionEnabled"/> for the row-click channel).
    /// </summary>
    /// <remarks>
    /// <para>Under <see cref="SelectionMode.Multiple"/>, a Shift-click selects every row between the last row
    /// selected by a plain or toggling click and the row clicked, in either loading mode, rows that were never
    /// rendered included.</para>
    /// <para>A range spanning a very large set materializes that many rows in one selection.</para>
    /// </remarks>
    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.GetDefaultValue();

    /// <summary>
    /// When <see langword="true"/> (the default), clicking anywhere on a row selects it — the row-click channel.
    /// Set to <see langword="false"/> to make the row-click channel display-only: rows still highlight the current
    /// selection but a click does not change it (column-only selection, where a select column keeps selecting at
    /// the active <see cref="SelectionMode"/> cardinality).
    /// </summary>
    [Parameter]
    public bool RowClickSelectionEnabled { get; set; } = true;

    /// <summary>
    /// The collection of currently selected items. Empty by default, so an unset parameter means "nothing
    /// selected".
    /// </summary>
    /// <remarks>
    /// <para>Applied whenever its instance changes, for the whole lifetime of the table — the same rule
    /// <see cref="FilterState"/> and <see cref="SortingState"/> follow. Handing over the same instance again is
    /// no command, so a value that never changes behaves as the selection the table starts with.</para>
    /// <para>Because the instance is what makes a command, <b>mutating the supplied list in place does
    /// nothing</b>. <c>selected.Add(item)</c> and <c>selected.Clear()</c> are invisible to the table; replace
    /// the value instead — <c>selected = [.. items];</c> — which is also what the table itself hands back, so a
    /// binding stays in step without any copying by the consumer.</para>
    /// <para>The table never mutates what it is given: it copies the contents on apply and reports a fresh
    /// list through <see cref="SelectedItemsChanged"/>, so the supplied instance is safe to keep and compare
    /// against.</para>
    /// </remarks>
#pragma warning disable CA2227 // Collection properties should be read only — Blazor parameters require a setter for data binding
    [Parameter]
    public List<TItem> SelectedItems { get; set; } = [];
#pragma warning restore CA2227

    /// <summary>
    /// Occurs when <see cref="SelectedItems"/> changes, enabling two-way binding.
    /// </summary>
    [Parameter]
    public EventCallback<List<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// An optional predicate evaluated before an item is selected.
    /// Receives the candidate item and the current selection snapshot;
    /// return <see langword="false"/> to prevent the item from being selected.
    /// When <see langword="null"/>, all items are selectable.
    /// </summary>
    [Parameter]
    public Func<SelectionRequest<TItem>, bool>? ItemSelectionAllowed { get; set; }

    /// <summary>
    /// An optional <see cref="ITableDragPayloadProvider{TItem}"/> narrowing what a drag carries. When
    /// <see langword="null"/>, the rows the table resolved are carried unchanged.
    /// </summary>
    [Parameter]
    public ITableDragPayloadProvider<TItem>? DragPayloadProvider { get; set; }

    /// <summary>
    /// An optional selector returning the stable, unique identity of a row's data item.
    /// When supplied, selection identity is compared by key; this lets selection survive a data reload
    /// that returns fresh instances (e.g. a virtualized provider). When <see langword="null"/>, identity
    /// falls back to the default equality of <typeparamref name="TItem"/>. A supplied selector must return
    /// a non-null key for every row.
    /// </summary>
    [Parameter]
    public Func<TItem, object>? ItemIdSelector { get; set; }

    private bool InstancesAreStable => ItemsProvider is IReturnsStableInstances;

    // The column ids whose state the table may hold: the columns it has, minus the ones it hides. Deliberately
    // the only definition of that set — a filter or sorting keyed to anything outside it has no header to
    // display or clear it.
    private HashSet<string> ShownColumnIds => _columnLayout.ShownColumnIds;

    // The placeholder is keyed on the same count the footer reports, so body and footer never disagree, and it
    // waits for the first provider response so a table still loading does not flash "No data" before its rows
    // arrive.
    // Also with every column hidden: the placeholder carries the table's tab stop, and a body of rows with no
    // cells in them has nothing else that can.
    private bool NoDataPlaceholderVisible
        => (ItemSource.ItemsProvided && ItemSource.TotalItemCount == 0) || _columnLayout.AllColumnsHidden;

    private int NoDataPlaceholderColumnSpan => _columnLayout.PlaceholderColumnSpan;

    private event Action? InternalItemsChanged;

    event Action? IAdvancedTable<TItem>.ItemsChanged
    {
        add => InternalItemsChanged += value;
        remove => InternalItemsChanged -= value;
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (!ReferenceEquals(FilterState, _previousFilterState) && !ReferenceEquals(FilterState, _appliedFilterState))
            _pendingFilterState = true;

        if (!ReferenceEquals(SortingState, _previousSortingState) && !ReferenceEquals(SortingState, _appliedSortingState))
            _pendingSortingState = true;

        _previousFilterState = FilterState;
        _previousSortingState = SortingState;

        // The parameter only seeds the authoritative copy, and only when the consumer swaps the reference.
        if (_previousSelectedItems != SelectedItems)
        {
            _selectedItems = [.. SelectedItems];
            _selectedItemSet = null;
            _previousSelectedItems = SelectedItems;
        }

        if (!ReferenceEquals(_previousItemIdSelector, ItemIdSelector))
        {
            _selectionComparer = ItemIdComparer.For(ItemIdSelector);
            _selectedItemSet = null;
            _previousItemIdSelector = ItemIdSelector;
        }

        LogSelectionMisconfiguration();

        var modeChanged = _previousMode != Mode;

        // Nothing to re-fetch before the first column-dependent pass: the first provide waits for the columns
        // and happens in OnAfterRenderAsync. A provider swap keeps the applied filter and sorting state — it
        // changes where rows come from, not which view of them the user chose.
        var needsRefresh = _columnDependentStateApplied
                           && (!ReferenceEquals(_previousItemsProvider, ItemsProvider) || modeChanged);

        // Switching how rows load must not deselect them.
        if (needsRefresh)
            await RefreshAsync(allowSelectionPrune: !modeChanged);

        _previousItemsProvider = ItemsProvider;
        _previousMode = Mode;
    }

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
        // The whole body is dispatched, not only the render request: the fetch below writes the item list and
        // the cancellation source, and under Virtualize it re-enters the component. Running inline when the
        // caller is already on the renderer's context, this costs nothing in the common case.
        => InvokeAsync(async () =>
        {
            if (!await ApplyFilterStateAsync(filterState))
                return;

            await RefreshAsync(allowSelectionPrune: false);

            // A call from outside is not an event the renderer knows about, so the active filter icon and the
            // rows would otherwise keep showing the previous filter until something else caused a render.
            StateHasChanged();
        });

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
        => InvokeAsync(async () =>
        {
            if (!await ApplySortingStateAsync(sortingState))
                return;

            await RefreshAsync(allowSelectionPrune: false);

            // See SetFilterStateAsync: nothing renders the new sort indicator unless this asks for it.
            StateHasChanged();
        });

    // The single writer of _appliedFilterState, whatever the origin. Reports whether the value actually moved,
    // which is what lets one pass apply a filter and a sorting for one provide instead of two.
    private async Task<bool> ApplyFilterStateAsync(FilterState filterState)
    {
        if (_disposed)
            return false;

        var reduced = DropUnknownColumnFilters(filterState);

        // FilterState is built so identity answers "did anything change": a copy-method that changes nothing returns
        // the same instance, and emptying one returns FilterState.Empty. Without that second part, clearing the last
        // entry would read as a change.
        if (ReferenceEquals(reduced, _appliedFilterState))
            return false;

        _appliedFilterState = reduced;

        InvalidateSelectionAnchor();

        await FilterStateChanged.InvokeAsync(reduced);

        return true;
    }

    // The sorting counterpart of ApplyFilterStateAsync; see there for why identity is the comparison.
    private async Task<bool> ApplySortingStateAsync(SortingState sortingState)
    {
        if (_disposed)
            return false;

        var reduced = DropUnknownColumnSortings(sortingState);

        if (ReferenceEquals(reduced, _appliedSortingState))
            return false;

        _appliedSortingState = reduced;

        InvalidateSelectionAnchor();

        await SortingStateChanged.InvokeAsync(reduced);

        return true;
    }

    private void InvalidateSelectionAnchor()
    {
        _selectionAnchor = null;
        _selectionAnchorIndex = null;
    }

    // Removed rather than ignored: a column the table does not show — absent or hidden — has no header to
    // display or clear its filter, so keeping it would narrow the rows with no way to find out why. Global
    // filters carry no column id and are never candidates.
    private FilterState DropUnknownColumnFilters(FilterState filterState)
    {
        var shownColumnIds = ShownColumnIds;

        List<string> droppedColumnIds = [.. filterState.Filters.OfType<IColumnFilter>()
            .Select(columnFilter => columnFilter.ColumnId)
            .Where(columnId => !shownColumnIds.Contains(columnId))];

        foreach (var columnId in droppedColumnIds)
        {
            filterState = filterState.WithoutColumnFilter(columnId);

            if (_columnLayout.Find(columnId) is not null)
                continue; // The column exists but the user hid it, so there is no mistake to report.

            if (_loggedDroppedFilterColumnIds.Add(columnId))
                DroppedFilterForUnknownColumn(Logger, nameof(AdvancedTable<>), columnId, nameof(SetFilterStateAsync));
        }

        return filterState;
    }

    // The sorting counterpart of DropUnknownColumnFilters; see there for why it removes rather than ignores.
    private SortingState DropUnknownColumnSortings(SortingState sortingState)
    {
        var shownColumnIds = ShownColumnIds;

        List<string> droppedColumnIds = [.. sortingState.ColumnSortings
            .Select(columnSorting => columnSorting.ColumnId)
            .Where(columnId => !shownColumnIds.Contains(columnId))];

        foreach (var columnId in droppedColumnIds)
        {
            sortingState = sortingState.WithoutColumnSorting(columnId);

            if (_columnLayout.Find(columnId) is not null)
                continue; // The column exists but the user hid it, so there is no mistake to report.

            if (_loggedDroppedSortingColumnIds.Add(columnId))
                DroppedSortingForUnknownColumn(Logger, nameof(AdvancedTable<>), columnId, nameof(SetSortingStateAsync));
        }

        return sortingState;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, true, false))
            return;

        if (_tableItemSource is not null)
            await _tableItemSource.DisposeAsync();

        if (_jsSession is not null)
            await _jsSession.DisposeAsync();
    }

    /// <summary>
    /// Commits the pixel widths JavaScript computed, so they survive Blazor re-renders. Widths of columns no
    /// longer present are ignored. <paramref name="userFixedColumnId"/> names the column the user pinned by
    /// dragging its resize handle, if any.
    /// </summary>
    [JSInvokable]
    public Task SetColumnWidthsAsync(IReadOnlyCollection<ColumnWidth> columnWidths, string? userFixedColumnId)
    {
        _columnLayout.ApplyWidths(columnWidths, userFixedColumnId);

        return InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender || _pendingColumnsChanged
            || _pendingFilterState || _pendingSortingState)
        {
            _pendingColumnsChanged = false;

            await ApplyColumnDependentStateAsync();

            await InvokeAsync(StateHasChanged);
        }

        await EnsureJsAttachedAsync();
        await NotifyColumnsChangedAsync();
        await UpdatePinnedOffsetsAsync();

        // Cleared on the first render after an activation whether or not the handler produced anything, so a
        // handler that renders nothing cannot leave the focus to be moved by an unrelated render later on.
        if (_cellEntryPending)
            await EnterFocusedCellAsync();
    }

    // Writing the CSS custom properties triggers no Blazor render, so this cannot loop.
    private async Task UpdatePinnedOffsetsAsync()
    {
        if (!_columnLayout.PinnedOffsetsDirty || !JsSession.IsAttached || _disposed
            || !_columnLayout.HasPinnedColumns)
        {
            return;
        }

        // A failure below means the circuit is gone, so a retry on the next render would only fail again.
        _columnLayout.PinnedOffsetsDirty = false;

        await JsSession.UpdatePinnedOffsetsAsync();
    }

    // Movement is JavaScript's, so the position it keeps has to be sent back from here. Left standing across a
    // reshape, it would name whichever item ended up in that row and a gesture would act on that one. The stop
    // moves to the default cell rather than going away, so the body never ends up with none.
    private Task ResetFocusedCellAsync()
    {
        if (_disposed)
            return Task.CompletedTask;

        return JsSession.ResetFocusedCellAsync();
    }

    // Focus reaches a cell's content only once the render the activation handler caused has landed, because
    // that is when a consumer swapping an editor into the cell has produced something to focus.
    private Task EnterFocusedCellAsync()
    {
        _cellEntryPending = false;

        if (_disposed)
            return Task.CompletedTask;

        return JsSession.EnterFocusedCellAsync();
    }

    // Columns register while the table first renders, so state naming them can only be settled here: settle
    // first, then provide, so an entry for a column that never appeared cannot cause a fetch of its own.
    // firstRender is part of the trigger because a table with no columns never raises a pending flag and still
    // has to load.
    private async Task ApplyColumnDependentStateAsync()
    {
        // A pending parameter is re-read here rather than captured when its flag was raised, so several
        // parameter passes before one render collapse to the latest value. Without a pending command the
        // applied state is re-checked against the columns as they now stand.
        var filterState = _pendingFilterState ? FilterState : _appliedFilterState;
        var sortingState = _pendingSortingState ? SortingState : _appliedSortingState;

        _pendingFilterState = false;
        _pendingSortingState = false;
        _columnDependentStateApplied = true;

        // Both apply before the fetch, so a batch that changed each of them costs one provide rather than two.
        var changed = await ApplyFilterStateAsync(filterState);
        changed |= await ApplySortingStateAsync(sortingState);

        // Under virtualization the Virtualize component issues the first request itself and reads the applied
        // state when it does, so only a change has to push it into re-requesting.
        if (changed || (!ItemSource.ItemsProvided && Mode == TableLoadingMode.All))
            await RefreshAsync(allowSelectionPrune: false);
    }

    private Task NotifyColumnsChangedAsync()
    {
        if (!JsSession.IsAttached || _disposed)
            return Task.CompletedTask;

        var columnIds = ShownColumnIds;

        if (_sizedColumnIds.SetEquals(columnIds))
            return Task.CompletedTask;

        _sizedColumnIds = columnIds;

        return JsSession.ColumnsChangedAsync();
    }

    private async Task EnsureJsAttachedAsync()
    {
        if (JsSession.IsAttached || _disposed)
            return;

        await JsSession.AttachAsync(_tableElement);

        // Seeded here rather than on the first columnsChanged: the module measures the columns as part of
        // attaching, so the set it has just sized is the one rendered now.
        if (JsSession.IsAttached)
            _sizedColumnIds = ShownColumnIds;
    }

    /// <inheritdoc/>
    protected override bool ShouldRender()
    {
        if (_ignoreNextRender)
        {
            _ignoreNextRender = false;

            return false;
        }

        return true;
    }

    int IAdvancedTable<TItem>.GetTotalItemCount() => ItemSource.TotalItemCount;

    /// <inheritdoc/>
    int IAdvancedTable<TItem>.CountSelected(IReadOnlyCollection<TItem> items)
    {
        if (_selectedItems.Count == 0 || items.Count == 0)
            return 0;

        // One comparer-backed set per call keeps the count O(items + selected) instead of scanning the
        // selection list per item. Callers are event-driven (selection or items changed), never per-frame,
        // so the per-call set allocation is not cached across calls.
        var selectedSet = _selectedItems.ToHashSet(_selectionComparer);

        return items.Count(selectedSet.Contains);
    }

    async Task IAdvancedTable<TItem>.SelectAsync(params IReadOnlyCollection<TItem> items)
    {
        List<TItem> updated = [.. _selectedItems];
        var changed = false;

        // A comparer-backed set mirrors the growing selection so the "already selected?" test is a lookup.
        // Scanning the selection list per candidate instead would cost O(items × selected), which a bulk
        // select over every visible row hits head-on.
        var selectedSet = updated.ToHashSet(_selectionComparer);

        foreach (var item in items)
        {
            if (selectedSet.Contains(item))
                continue;

            if (!ItemSelectionAllowedInternal(item, updated))
                continue;

            updated.Add(item);
            selectedSet.Add(item);
            changed = true;
        }

        if (changed)
            await CommitSelectionAsync(updated);
    }

    Task IAdvancedTable<TItem>.SelectSingleAsync(TItem item)
        => SelectSingleAsync(item);

    async Task IAdvancedTable<TItem>.DeselectAsync(params IReadOnlyCollection<TItem> items)
    {
        // Collect the removals into a comparer-backed set and rebuild the selection in one pass. Removing
        // them one at a time would rescan and re-shift the whole selection per item (O(items × selected)),
        // which a bulk deselect over every visible row hits head-on.
        var removals = items.ToHashSet(_selectionComparer);

        List<TItem> updated = [.. _selectedItems.Where(item => !removals.Contains(item))];

        if (updated.Count != _selectedItems.Count)
            await CommitSelectionAsync(updated);
    }

    void IAdvancedTable<TItem>.RegisterColumn(IAdvancedTableColumn<TItem> column)
    {
        // Registered first, and it throws on a duplicate id before mutating anything: a rejection that had
        // already paired the column with a state would leave the column the table refused half-registered.
        _columnLayout.Add(column);

        var columnState = new ColumnState();
        _columnStates[column] = columnState;

        // The column subscribes first, on purpose. Handlers run in subscription order, and the table's answer to
        // a visibility change reads the column's own Visible flag back, so subscribing the table first would make
        // it act on the value the column still had.
        column.SetState(columnState);

        columnState.VisibleChanged += ColumnStateVisibleChangedAsync;
        columnState.PinSideChanged += ColumnStatePinSideChanged;

        _pendingColumnsChanged = true;
    }

    void IAdvancedTable<TItem>.UnregisterColumn(IAdvancedTableColumn<TItem> column)
    {
        if (_disposed)
            return;

        if (!_columnLayout.Remove(column))
            return;

        if (_columnStates.Remove(column, out var columnState))
        {
            columnState.VisibleChanged -= ColumnStateVisibleChangedAsync;
            columnState.PinSideChanged -= ColumnStatePinSideChanged;
        }

        // Goes with the state it carries: a column re-registering under the same id gets a new state, and a
        // context left behind here would hand its header the dropped one.
        _columnHeaderContexts.Remove(column);

        // Undo the pairing RegisterColumn set up, so the column stops holding — and writing into — a state the
        // table has already dropped.
        column.SetState(null);

        // Otherwise the header of a column re-registering under the same id can never be drag-reordered again.
        if (_openFilterColumnId == column.Id)
            _openFilterColumnId = null;

        _pendingColumnsChanged = true;
    }

    // Hiding a column takes its header away, so the column stops being one whose state the table may hold.
    // Re-running the ordinary reduction is all that takes: it drops the filter *and* the sorting keyed to
    // that column, announces the change and re-fetches, exactly as it would for a column that was removed.
    // Global filters carry no column id and survive.
    // Every visibility change ends here, whichever side started it — the column chooser or a Visible parameter
    // set from markup — because both of them go through the column's state.
    // The keyboard position goes ahead of that reduction rather than with the re-fetch it may trigger, so a
    // column carrying no filter and no sorting behaves like one that carries both.
    private async Task ApplyColumnVisibilityAsync()
    {
        _columnLayout.Rebuild();

        await ResetFocusedCellAsync();

        var changed = await ApplyFilterStateAsync(_appliedFilterState);
        changed |= await ApplySortingStateAsync(_appliedSortingState);

        if (changed)
            await RefreshAsync(allowSelectionPrune: false);

        await InvokeAsync(StateHasChanged);
    }

    // A column reports a Visible parameter set from markup through its state; without this the table would
    // keep rendering the previous set of columns, because building the header reads the columns before their
    // own parameters are applied and nothing else schedules a further render.
    // The pass is wrapped because the event this handles is an Action, so the handler can only be async void
    // and nothing above it can observe a failure: an ItemsProvider or a state-changed callback that throws
    // would tear the circuit down instead of surfacing, which the same code reached through SetFilterStateAsync
    // never does.
    private async void ColumnStateVisibleChangedAsync(ColumnState columnState)
    {
        try
        {
            if (_disposed)
                return;

            await InvokeAsync(ApplyColumnVisibilityAsync);
        }
        catch (OperationCanceledException)
        {
            // Nothing to do here, return gracefully
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the check above and the renderer is gone, nothing we can do, return gracefully
        }
        catch (Exception exception)
        {
            // The columns still render at their new visibility; only the filter, sorting and re-fetch the
            // change asked for did not complete.
            ColumnVisibilityPassFailed(Logger, exception, nameof(AdvancedTable<>));
        }
    }

    // A write straight into the state re-renders nothing on its own, so the render showing the new grouping
    // has to be asked for here. Disposal stays subscribed and can complete between the check and the await.
    private async void ColumnStatePinSideChanged(ColumnState columnState)
    {
        if (_disposed)
            return;

        _columnLayout.Rebuild();

        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced the check above and the renderer is gone, nothing we can do, return gracefully
        }
    }

    // Qualified because Virtualize's result type and the table's own ItemsProviderResult share a name: this one is
    // what Virtualize wants back, the table's own is what a consumer's provider returns.
    private async ValueTask<Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderResult<TItem>> VirtualizationItemsProviderAsync(
        ItemsProviderRequest request)
    {
        ItemSource.SetWindow(request.StartIndex, request.Count);

        // Virtualize never prunes the selection (pruning is gated to All-mode), so the flag is irrelevant here.
        await RefreshItemsAsync(allowSelectionPrune: false);

        return new Microsoft.AspNetCore.Components.Web.Virtualization.ItemsProviderResult<TItem>(InputItems, ItemSource.TotalItemCount);
    }

    private async Task RefreshAsync(bool allowSelectionPrune)
    {
        // A reload replaces which rows exist, so a remembered keyboard position would point at whichever
        // item ends up there. Reset here rather than on every fetch: a virtualization window refill is the
        // same rows scrolling past, and moving the position on one of those would throw the keyboard out of
        // the row the user is in on every wheel-scroll.
        await ResetFocusedCellAsync();

        // virtualization takes care of refreshing the data by itself and just needs to be triggered
        if (Mode == TableLoadingMode.Virtualize)
        {
            if (_virtualizeComponent is not null)
                await _virtualizeComponent.RefreshDataAsync();
        }
        else
        {
            await RefreshItemsAsync(allowSelectionPrune);
        }
    }

    // Note: Does not trigger refresh UI when using virtualization -> use sibling refresh method instead depending on usecase
    private async Task RefreshItemsAsync(bool allowSelectionPrune)
    {
        var windowed = Mode != TableLoadingMode.All;

        // A load overtaken by a newer one leaves the previous window standing, so there is nothing to follow up.
        if (!await ItemSource.LoadAsync(ItemsProvider, _appliedFilterState, _appliedSortingState, windowed))
            return;

        // Drop the row states that are no longer used, which would otherwise pile up, and renumber the
        // positions the keyboard steps by.
        RemoveUnusedRowStates(InputItems);
        RebuildAbsoluteRowIndexes();

        // The table's own render block clears these, but a virtualization refill re-renders the rows
        // without the table rendering at all — so the numbers of the window just replaced would survive
        // and a gesture on a cell that is about to be patched would still resolve.
        ClearRowSequences();

        // Pruning is gated by the caller: it only runs on a genuine data reload in All-mode, never after a
        // filter/sort/mode change, where a shrunken result means rows are hidden, not gone. A failed load
        // empties the body for the same reason, so it does not prune either.
        var pruneSelection = allowSelectionPrune && !ItemSource.LastLoadFailed
                             && Mode == TableLoadingMode.All && _selectedItems.Count > 0;

        if (pruneSelection)
        {
            var itemSet = InputItems.ToHashSet(_selectionComparer);
            var cleaned = _selectedItems.Where(itemSet.Contains).ToList();
            if (cleaned.Count != _selectedItems.Count)
                await CommitSelectionAsync(cleaned);
        }

        // Raised after a failure too, so the footer count follows the emptied body instead of standing at the
        // last count.
        InternalItemsChanged?.Invoke();
    }

    // Built once per column and kept, so the header it is cascaded to holds one instance for the column's
    // lifetime. The callback is raised by the header after it drives the reorder itself; its receiver is the
    // table, which is what re-renders the whole header row.
    private ColumnHeaderContext<TItem> GetHeaderContext(IAdvancedTableColumn<TItem> column)
    {
        if (_columnHeaderContexts.TryGetValue(column, out var headerContext))
            return headerContext;

        headerContext = new ColumnHeaderContext<TItem>(column, _columnStates[column], JsSession, ColumnReorder,
            EventCallback.Factory.Create(this, StateHasChanged));

        _columnHeaderContexts[column] = headerContext;

        return headerContext;
    }

    private EventCallback<SortRequest> GetSortRequestedCallback(IAdvancedTableColumn<TItem> column)
        => EventCallback.Factory.Create<SortRequest>(this, request => UpdateSortingStateAsync(column, request));

    // The header worked out the direction and whether the gesture was additive; applying it is all that is
    // left. A header only asks for a column that can be sorted, so nothing is re-checked here.
    private Task UpdateSortingStateAsync(IAdvancedTableColumn<TItem> column, SortRequest request)
    {
        var baseSortingState = SortingState.Empty;

        if (request.Additive)
            baseSortingState = _appliedSortingState;

        return SetSortingStateAsync(baseSortingState.WithColumnSorting(column.Id, request.Ascending));
    }

    // Null when the column is not sorted by, which is what leaves the header showing no indicator and
    // aria-sort="none".
    private bool? GetSortDirection(IAdvancedTableColumn<TItem> column)
        => _appliedSortingState.ColumnSortings
            .FirstOrDefault(columnSorting => columnSorting.ColumnId == column.Id)?.Ascending;

    private void HandleHover(RowState? rowState)
    {
        // Blazor always rerenders the whole component when using @onpointer<...> which we do not want here
        // -> suppress rerender since we handle it explicitly where required
        _ignoreNextRender = true;

        if (rowState == _previousHoveredRowState)
            return;

        rowState?.Hovered = true;
        _previousHoveredRowState?.Hovered = false;

        _previousHoveredRowState = rowState;
    }

    private void RemoveUnusedRowStates(IReadOnlyCollection<TItem> items)
    {
        var rowKeys = items.Select(GetRowKey).ToHashSet();

        _rowStates.Keys.Except(rowKeys)
            .ToList()
            .ForEach(key => _rowStates.Remove(key));
    }

    // Numbers one rendered row and records what it shows. Called while the row template runs, so every
    // rendered cell carries a number the keyboard seam can resolve, and only the rows of the current pass
    // are resolvable at all.
    private int RegisterRowSequence(TItem item)
    {
        var rowSequence = _nextRowSequence++;

        _itemsByRowSequence[rowSequence] = item;

        return rowSequence;
    }

    // Drops the numbers the previous pass handed out. The counter is deliberately left where it is: reusing
    // a number would let a cell that is still in the DOM from an earlier pass resolve to a different item,
    // which is the one failure this indirection exists to rule out.
    private void ClearRowSequences()
        => _itemsByRowSequence.Clear();

    private void RebuildAbsoluteRowIndexes()
    {
        // Two items a consumer tells apart by its id selector keep two positions the keyboard can reach.
        _absoluteRowIndexes = new Dictionary<TItem, int>(_selectionComparer);

        var windowStart = GetWindowStartIndex();
        var position = 0;

        foreach (var item in InputItems)
        {
            _absoluteRowIndexes[item] = windowStart + position;
            position++;
        }
    }

    // The row's position in the filtered, sorted set, which is what keyboard movement steps by and scrolls
    // to. Negative for an item the loaded window does not hold, which leaves that row unreachable by the
    // keyboard for one pass instead of sending it to a position that belongs to another row.
    private int GetAbsoluteRowIndex(TItem item)
    {
        if (_absoluteRowIndexes.TryGetValue(item, out var absoluteRowIndex))
            return absoluteRowIndex;

        return -1;
    }

    // Keyed by GetRowKey, the identity the row component itself is keyed by. Keying by instance instead would
    // hand a preserved row a fresh RowState after a refetch returned equal-but-distinct items, leaving the
    // cell that subscribed in OnInitialized listening to a state nothing raises any more.
    private RowState GetRowState(TItem item)
    {
        var rowKey = GetRowKey(item);

        if (!_rowStates.TryGetValue(rowKey, out var rowState))
        {
            rowState = new RowState();

            _rowStates[rowKey] = rowState;
        }

        return rowState;
    }

    private IColumnFilter? GetColumnFilter(string columnId)
        => _appliedFilterState.Filters.OfType<IColumnFilter>()
            .FirstOrDefault(columnFilter => columnFilter.ColumnId == columnId);

    private EventCallback<IColumnFilter?> GetColumnFilterChangedCallback(IAdvancedTableColumn<TItem> column)
        => EventCallback.Factory.Create<IColumnFilter?>(this,
            filter => ColumnFilterChangedAsync(column.Id, filter));

    private EventCallback<bool> GetFilterOpenChangedCallback(IAdvancedTableColumn<TItem> column)
        => EventCallback.Factory.Create<bool>(this, open => FilterOpenChanged(column.Id, open));

    private Task ColumnFilterChangedAsync(string columnId, IColumnFilter? filter)
    {
        if (filter is null)
            return SetFilterStateAsync(_appliedFilterState.WithoutColumnFilter(columnId));

        return SetFilterStateAsync(_appliedFilterState.WithColumnFilter(filter));
    }

    // Suppresses native column drag-to-reorder on a header while its filter panel is open, so an
    // in-panel text drag cannot start a reorder. The icon press-drag is handled separately in
    // AdvancedTable.razor.ts.
    private void FilterOpenChanged(string columnId, bool open)
    {
        if (open)
            _openFilterColumnId = columnId;
        else if (_openFilterColumnId == columnId)
            _openFilterColumnId = null;
    }

    // A header cannot start a reorder while its own filter panel is open, so an in-panel text drag cannot
    // turn into one. The icon press-drag is handled separately in AdvancedTable.razor.ts.
    private bool IsColumnDragEnabled(IAdvancedTableColumn<TItem> column)
        => _openFilterColumnId != column.Id;

    private Task RowDoubleClickAsync(TItem item)
        => RowDoubleClick.InvokeAsync(item);

    // A row carries no context-menu listener while RowContextMenuRequested is unwired: an attached
    // listener would send every right-click in the table across to .NET only to find nothing to invoke.
    private EventCallback<MouseEventArgs> GetRowContextMenuCallback(TItem item)
    {
        // default, not EventCallback<T>.Empty — Empty carries a no-op delegate, which still attaches.
        if (!RowContextMenuRequested.HasDelegate)
            return default;

        return EventCallback.Factory.Create<MouseEventArgs>(this,
            args => RowContextMenuRequestedAsync(item, args));
    }

    private Task RowContextMenuRequestedAsync(TItem item, MouseEventArgs args)
        => RowContextMenuRequested.InvokeAsync(new RowContextMenuEventArgs<TItem>(item, args));

    /// <summary>
    /// Applies a selection gesture to the row a rendered cell belongs to. Called from JavaScript, which owns
    /// keyboard movement: the row is named by the sequence number its cells carry, and the two modifiers say
    /// which gesture it was — neither set for a plain selection, <paramref name="ctrlKey"/> for a toggle,
    /// <paramref name="shiftKey"/> for a range measured from the anchor.
    /// </summary>
    /// <param name="rowSequence">The number the cell's <c>data-row-sequence</c> attribute carries.</param>
    /// <param name="ctrlKey">Whether the gesture was made with the Ctrl key held.</param>
    /// <param name="shiftKey">Whether the gesture was made with the Shift key held.</param>
    /// <remarks>
    /// Routes into the very paths a mouse click routes into, so the two channels cannot disagree. A number
    /// no longer resolvable — its row was rendered before the last reload — does nothing.
    /// </remarks>
    [JSInvokable]
    public async Task SelectRowAsync(int rowSequence, bool ctrlKey, bool shiftKey)
    {
        if (_disposed || !_itemsByRowSequence.TryGetValue(rowSequence, out var item))
            return;

        await ApplySelectionGestureAsync(item, ctrlKey, shiftKey);

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Raises <see cref="AdvancedTableColumnBase{TItem}.CellActivated"/> on the column of a rendered cell,
    /// with the item its row shows. Called from JavaScript when the cell the keyboard sits on is activated.
    /// </summary>
    /// <param name="rowSequence">The number the cell's <c>data-row-sequence</c> attribute carries.</param>
    /// <param name="columnId">The id of the column the cell belongs to.</param>
    /// <remarks>
    /// <para>A column with no handler wired raises nothing. JavaScript cannot tell the two apart without a
    /// contract naming every wired column, so it always calls and the cost of an unhandled activation is one
    /// wasted round trip on a deliberate single key press.</para>
    /// <para>Either way the focus then moves into the cell's first focusable content, if it has any, which is
    /// what entering a cell means. Entering is not cancelable: a consumer who must keep the keyboard out of a
    /// cell renders nothing focusable in it, or moves the focus themselves from the handler.</para>
    /// </remarks>
    [JSInvokable]
    public async Task ActivateCellAsync(int rowSequence, string columnId)
    {
        if (_disposed || !_itemsByRowSequence.TryGetValue(rowSequence, out var item))
            return;

        var column = _columnLayout.Find(columnId);

        if (column is not AdvancedTableColumnBase<TItem> { CellActivated.HasDelegate: true } activatableColumn)
        {
            // Nothing is going to re-render, so the content already in the cell is what the focus lands on.
            await EnterFocusedCellAsync();

            return;
        }

        // A consumer typically answers by swapping an editor into the cell, which is not there to be focused
        // until the render that answer causes has been applied.
        _cellEntryPending = true;

        await activatableColumn.CellActivated.InvokeAsync(item);
    }

    private Task HandleRowClickAsync(TItem item, MouseEventArgs args)
        => ApplySelectionGestureAsync(item, args.CtrlKey, args.ShiftKey);

    // The one place a selection gesture is interpreted, whatever raised it. The click channel and the
    // keyboard channel go through here rather than each holding their own copy, so the two agree by
    // construction instead of by two bodies that drift apart.
    private async Task ApplySelectionGestureAsync(TItem item, bool ctrlKey, bool shiftKey)
    {
        // Row-click channel is display-only when disabled — the gesture does not change the selection
        // (column-only selection).
        if (!RowClickSelectionEnabled)
            return;

        if (SelectionMode == SelectionMode.Multiple && shiftKey && _selectionAnchor is not null)
        {
            var range = await BuildRangeSelectionAsync(item);

            await CommitSelectionAsync(range);
        }
        else if (SelectionMode == SelectionMode.Multiple && ctrlKey)
        {
            await ToggleSelectionAsync(item);
        }
        else
        {
            await SelectSingleAsync(item);
        }
    }

    // Adds the item to the selection or takes it out, and moves the anchor to it either way: the next range
    // gesture measures from the row this one acted on.
    private async Task ToggleSelectionAsync(TItem item)
    {
        List<TItem> updated = [.. _selectedItems];

        if (updated.Contains(item, _selectionComparer))
            updated.RemoveAll(x => _selectionComparer.Equals(x, item));
        else if (ItemSelectionAllowedInternal(item, updated))
            updated.Add(item);
        else
            return;

        SetSelectionAnchor(item);

        await CommitSelectionAsync(updated);
    }

    // Resolves the drag payload once at drag start and applies the click-like selection mutation.
    // Dragged row already selected → carry the whole selection, no mutation. Unselected → single-select
    // it exactly like a click, gated by the row-click channel and the selection veto; when the gate
    // blocks, no mutation and the payload is just that row.
    /// <inheritdoc/>
    async Task<IReadOnlyList<TItem>> ITableDragPayloadResolver<TItem>.ResolvePayloadAsync(TItem draggedRow)
    {
        if (_selectedItems.Contains(draggedRow, _selectionComparer))
            return GetDragPayload(draggedRow, [.. _selectedItems]);

        if (RowClickSelectionEnabled && ItemSelectionAllowedInternal(draggedRow, []))
            await SelectSingleAsync(draggedRow);

        return GetDragPayload(draggedRow, [draggedRow]);
    }

    // Nothing above this catches, so an exception from the consumer's provider would tear the circuit down.
    private IReadOnlyList<TItem> GetDragPayload(TItem item, IReadOnlyList<TItem> payload)
    {
        if (DragPayloadProvider is null)
            return payload;

        try
        {
            return DragPayloadProvider.GetPayload(item, payload);
        }
        catch (Exception exception)
        {
            DragPayloadProviderFailed(Logger, exception, nameof(AdvancedTable<>));

            return [];
        }
    }

    // Atomic single-select: replace the whole selection with this one item in one commit,
    // shared by row-click and the select-column cell under SelectionMode.Single.
    private async Task SelectSingleAsync(TItem item)
    {
        if (!ItemSelectionAllowedInternal(item, []))
            return;

        SetSelectionAnchor(item);

        await CommitSelectionAsync([item]);
    }

    private void SetSelectionAnchor(TItem item)
    {
        _selectionAnchor = item;
        _selectionAnchorIndex = GetAbsoluteIndex(item);
    }

    private int GetWindowStartIndex() => ItemSource.WindowStartIndex;

    private int? GetAbsoluteIndex(TItem item)
    {
        var loadedItems = InputItems as IReadOnlyList<TItem> ?? [.. InputItems];

        var indexInWindow = IndexOfByKey(loadedItems, item);
        if (indexInWindow < 0)
            return null;

        return GetWindowStartIndex() + indexInWindow;
    }

    private async Task<List<TItem>> BuildRangeSelectionAsync(TItem target)
    {
        // A reload can have moved the anchor row, so its current position wins wherever it is loaded.
        var loadedAnchorIndex = _selectionAnchor is { } anchor ? GetAbsoluteIndex(anchor) : null;
        var anchorPosition = loadedAnchorIndex ?? _selectionAnchorIndex;

        if (anchorPosition is not { } anchorIndex || GetAbsoluteIndex(target) is not { } targetIndex)
            return [.. _selectedItems];

        var rangeStart = Math.Min(anchorIndex, targetIndex);
        var rangeEnd = Math.Max(anchorIndex, targetIndex);

        var loadedItems = InputItems as IReadOnlyList<TItem> ?? [.. InputItems];

        var windowStart = GetWindowStartIndex();

        var windowEnd = windowStart + loadedItems.Count - 1;

        IReadOnlyList<TItem> rangeItems;
        int rangeStartIndex;

        if (rangeStart >= windowStart && rangeEnd <= windowEnd)
        {
            rangeItems = loadedItems;
            rangeStartIndex = windowStart;
        }
        else
        {
            var requestedCount = rangeEnd - rangeStart + 1;

            var fetchedItems = await FetchRangeAsync(rangeStart, requestedCount);
            if (fetchedItems is null)
                return [.. _selectedItems];

            rangeItems = fetchedItems;

            rangeStartIndex = fetchedItems.Count == requestedCount ? rangeStart : 0;
        }

        // Walk from the anchor toward the target so selection order follows click direction (up or down).
        var step = targetIndex >= anchorIndex ? 1 : -1;

        List<TItem> selected = [];

        for (var absoluteIndex = anchorIndex; ; absoluteIndex += step)
        {
            var positionInRange = absoluteIndex - rangeStartIndex;

            if (positionInRange >= 0 && positionInRange < rangeItems.Count)
            {
                var item = rangeItems[positionInRange];

                if (ItemSelectionAllowedInternal(item, selected))
                    selected.Add(item);
            }

            if (absoluteIndex == targetIndex)
                break;
        }

        return selected;
    }

    private Task<IReadOnlyList<TItem>?> FetchRangeAsync(int skip, int take)
        => ItemSource.FetchRangeAsync(ItemsProvider, _appliedFilterState, _appliedSortingState, skip, take);

    private async Task CommitSelectionAsync(List<TItem> items)
    {
        _selectedItems = items;
        _selectedItemSet = null;

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync([.. _selectedItems]);
    }

    private bool ItemSelectionAllowedInternal(TItem item, IReadOnlyList<TItem> currentSelection)
        => ItemSelectionAllowed?.Invoke(new SelectionRequest<TItem>(item, currentSelection)) ?? true;

    private bool IsRowSelected(TItem item)
        => IsItemSelected(item);

    /// <inheritdoc/>
    bool IAdvancedTable<TItem>.IsSelected(TItem item)
        => IsItemSelected(item);

    private bool IsItemSelected(TItem item)
    {
        _selectedItemSet ??= _selectedItems.ToHashSet(_selectionComparer);

        return _selectedItemSet.Contains(item);
    }

    private object GetRowKey(TItem item)
        => ItemIdSelector?.Invoke(item) ?? item;

    private int IndexOfByKey(IReadOnlyList<TItem> items, TItem item)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (_selectionComparer.Equals(items[i], item))
                return i;
        }

        return -1;
    }

    private void LogSelectionMisconfiguration()
    {
        var misconfigured = RowClickSelectionEnabled
            && Mode == TableLoadingMode.Virtualize
            && ItemIdSelector is null
            && !InstancesAreStable;

        if (misconfigured && !_loggedSelectionMisconfiguration)
        {
            SelectionIdentityNotTrackable(Logger, nameof(AdvancedTable<>));

            _loggedSelectionMisconfiguration = true;
        }
        else if (!misconfigured)
        {
            _loggedSelectionMisconfiguration = false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Component} dropped the filter for column '{ColumnId}': " +
        "the table has no such column, so the filter could neither be seen nor cleared. Apply it with " +
        "{Method} once the column exists.")]
    private static partial void DroppedFilterForUnknownColumn(ILogger logger, string component, string columnId,
        string method);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Component} dropped the sorting for column '{ColumnId}': " +
        "the table has no such column, so the sorting could neither be seen nor changed. Sort by it with " +
        "{Method} once the column exists.")]
    private static partial void DroppedSortingForUnknownColumn(ILogger logger, string component, string columnId,
        string method);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Component} failed to apply a column visibility change: the " +
        "filter and sorting reduction or the re-fetch that follows it threw.")]
    private static partial void ColumnVisibilityPassFailed(ILogger logger, Exception ex, string component);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Component} failed to decide the drag payload: the " +
        "supplied DragPayloadProvider threw. The drag carries no rows.")]
    private static partial void DragPayloadProviderFailed(ILogger logger, Exception ex, string component);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Component} cannot track selection identity: selection is " +
        "enabled under Virtualize with no ItemIdSelector over an instance-unstable provider. Selection will break " +
        "across fetches — supply an ItemIdSelector.")]
    private static partial void SelectionIdentityNotTrackable(ILogger logger, string component);
}
