using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;
using Shared.Pages.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;

namespace Shared.Pages.Tables.SimpleTable.GlobalFilter.Components;

public sealed partial class SimpleTableGlobalFilterPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(100);

    private string _searchText = string.Empty;

    private SimpleTable<ExampleTableItem>? _table;

    // Kept in step through FilterStateChanged so the search box composes its global filter onto whatever column
    // filters the headers hold.
    private FilterState _appliedFilterState = FilterState.Empty;

    private void AppliedFilterStateChanged(FilterState filterState)
        => _appliedFilterState = filterState;

    private Task SearchTextChangingAsync(string? text)
    {
        _searchText = text ?? string.Empty;

        // The table never sees the text — only the filter's Matches result. An empty box removes the global
        // filter (dedup by type); column filters are untouched either way.
        var filterState = string.IsNullOrWhiteSpace(_searchText)
            ? _appliedFilterState.WithoutGlobalFilter<ContainsGlobalFilter>()
            : _appliedFilterState.WithGlobalFilter(new ContainsGlobalFilter(_searchText));

        return _table?.SetFilterStateAsync(filterState) ?? Task.CompletedTask;
    }
}
