namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Encapsulates the result of a data retrieval operation, including the subset of items and the total record count.
/// </summary>
/// <param name="Items">The collection of data items retrieved for the current request.</param>
/// <param name="TotalItemCount">
/// The number of items matching the active filters across the whole set, before the viewport is applied — the
/// scroll extent for <c>Virtualize</c> and the count the footer shows.
/// </param>
public sealed record ItemsProviderResult<TItem>(IReadOnlyCollection<TItem> Items, int TotalItemCount);
