namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Represents a context for fetching/loading data, containing criteria for filtering, sorting, and optionally a range of items.
/// </summary>
public sealed record ItemsProviderContext
{
    /// <inheritdoc cref="Models.FilterState"/>
    public FilterState FilterState { get; init; }

    /// <inheritdoc cref="Models.SortingState"/>
    public SortingState SortingState { get; init; }

    /// <inheritdoc cref="Models.ItemRange"/>
    public ItemRange? ItemRange { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsProviderContext"/> record with optional filtering, sorting, and item range criteria.
    /// </summary>
    public ItemsProviderContext(FilterState? filterState = null, SortingState? sortingState = null,
        ItemRange? itemRange = null)
    {
        FilterState = filterState ?? FilterState.Empty;
        SortingState = sortingState ?? SortingState.Empty;
        ItemRange = itemRange;
    }
};
