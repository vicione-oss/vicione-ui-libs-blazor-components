using Shared.Pages.Tables.AdvancedTable.Models;
using Shared.Pages.Tables.AdvancedTable.Services;
using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.AdvancedTable.Filter.Interaction.Components;

public sealed partial class AdvancedTableFilterInteractionPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private readonly IItemsProvider<ExampleTableItem> _itemsProvider = new ExampleTableItemsProvider(s_items.AsQueryable());

    // One instance, handed over on every render and never replaced, so the table applies it once and owns the
    // filter from then on.
    private readonly FilterState _seedFilterState =
        FilterState.Empty.WithColumnFilter(new StartsWithColumnFilter("Key", "a"));

    // Observe only.
    private FilterState _observedFilterState = FilterState.Empty;

    // Both ends of a binding: this page writes it to drive the table, the table writes it back on every change.
    private FilterState _boundFilterState = FilterState.Empty;

    private AdvancedTable<ExampleTableItem>? _pushTargetTable;

    // Mirrored from FilterStateChanged purely so the page can list what the table applies.
    private FilterState _pushedFilterState = FilterState.Empty;

    private void ObservedFilterStateChanged(FilterState filterState)
        => _observedFilterState = filterState;

    private void PushedFilterStateChanged(FilterState filterState)
        => _pushedFilterState = filterState;

    // Writing the bound field is the whole command: the new instance reaches the table as a parameter.
    private void FilterBoundTableByKey()
        => _boundFilterState = FilterState.Empty.WithColumnFilter(new StartsWithColumnFilter("Key", "b"));

    private void ClearBoundFilter()
        => _boundFilterState = FilterState.Empty;

    // A call is applied whatever the parameter channel has already delivered, so clearing works even when the
    // table has seen the empty filter before.
    private Task ClearFiltersAsync()
        => _pushTargetTable?.SetFilterStateAsync(FilterState.Empty) ?? Task.CompletedTask;
}
