using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace Shared.Pages.Tables.SimpleTable.Filter.Interaction.Components;

public sealed partial class SimpleTableFilterInteractionPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    // One instance, handed over on every render and never replaced, so the table applies it once and owns the
    // filter from then on.
    private readonly FilterState _seedFilterState = FilterState.Empty.WithColumnFilter(
        new SimpleTableContainsColumnFilter<ExampleTableItem>("Value", "a", item => item.Value));

    // Observe only.
    private FilterState _observedFilterState = FilterState.Empty;

    // Both ends of a binding: this page writes it to drive the table, the table writes it back on every change.
    private FilterState _boundFilterState = FilterState.Empty;

    private SimpleTable<ExampleTableItem>? _pushTargetTable;

    // Mirrored from FilterStateChanged purely so the page can list what the table applies.
    private FilterState _pushedFilterState = FilterState.Empty;

    private void ObservedFilterStateChanged(FilterState filterState)
        => _observedFilterState = filterState;

    private void PushedFilterStateChanged(FilterState filterState)
        => _pushedFilterState = filterState;

    // Writing the bound field is the whole command: the new instance reaches the table as a parameter.
    private void FilterBoundTableByKey()
        => _boundFilterState = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<ExampleTableItem>("Key", "b", item => item.Key));

    private void ClearBoundFilter()
        => _boundFilterState = FilterState.Empty;

    // A call is applied whatever the parameter channel has already delivered, so clearing works even when the
    // table has seen the empty filter before.
    private Task ClearFiltersAsync()
        => _pushTargetTable?.SetFilterStateAsync(FilterState.Empty) ?? Task.CompletedTask;
}
