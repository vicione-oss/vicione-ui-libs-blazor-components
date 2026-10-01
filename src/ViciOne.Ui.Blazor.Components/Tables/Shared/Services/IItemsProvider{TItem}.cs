using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

/// <summary>
/// Invoked by the table whenever it needs to fetch new items, such as during the initial load, change of visible data range (e.g. due to pagination), sorting, or filtering events.
/// </summary>
public interface IItemsProvider<TItem>
    where TItem : class
{
    /// <summary>
    /// Asynchronously retrieves a subset of items based on the provided context, which may include filtering, sorting, and item range information.
    /// </summary>
    /// <remarks>
    /// <para>Superseding an in-flight provide cancels it, which happens whenever two loads follow each other
    /// quickly — the table cancels the running provide before it starts the newer one, for example when the user
    /// types in a filter box or scrolls a virtualized table faster than a provide completes.</para>
    /// <para>Rows that tie on every sorted column are the provider's to order. The built-in in-memory provider
    /// makes descending the exact reverse of ascending; another provider makes its own guarantee, or none.</para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown as soon as <paramref name="cancellationToken"/> is canceled: filtering and sorting are synchronous
    /// passes over the whole set, so a superseded provide gives up instead of finishing work nobody reads. Callers
    /// awaiting this method have to catch it.
    /// </exception>
    Task<ItemsProviderResult<TItem>> GetItemsAsync(ItemsProviderContext context, CancellationToken cancellationToken);
}
