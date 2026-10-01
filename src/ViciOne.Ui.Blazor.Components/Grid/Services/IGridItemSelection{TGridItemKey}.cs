using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Grid.Services;

/// <summary>
/// Represents a mutable selection of grid item keys of type <typeparamref name="TGridItemKey"/>.
/// </summary>
/// <typeparam name="TGridItemKey">
/// The key type that uniquely identifies a grid item.
/// </typeparam>
/// <remarks>
/// The selection is enumerable and exposes an update lock via <see cref="IHasUpdateLock"/>
/// to group multiple changes into a single update.
/// </remarks>
[Obsolete(Constants.ObsoleteMessage)]
public interface IGridItemSelection<TGridItemKey> : IEnumerable<TGridItemKey>, IHasUpdateLock
{
    /// <summary>
    /// Gets the number of items currently included in the selection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Occurs when the selection has changed.
    /// </summary>
    /// <remarks>
    /// The event argument provides the added and removed keys.
    /// </remarks>
    event Action<GridItemSelectionChangedEventArgs<TGridItemKey>>? Changed;

    /// <summary>
    /// Adds the specified <paramref name="item"/> to the selection.
    /// </summary>
    /// <param name="item">The item key to add.</param>
    /// <returns>
    /// <see langword="true"/> if the item was added; <see langword="false"/> if it was already present.
    /// </returns>
    bool Add(TGridItemKey item);

    /// <summary>
    /// Adds a range of <paramref name="items"/> to the selection.
    /// </summary>
    /// <param name="items">The item keys to add.</param>
    void AddRange(IEnumerable<TGridItemKey> items);

    /// <summary>
    /// Removes the specified <paramref name="item"/> from the selection.
    /// </summary>
    /// <param name="item">The item key to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the item was removed; <see langword="false"/> if it was not found.
    /// </returns>
    bool Remove(TGridItemKey item);

    /// <summary>
    /// Removes all items that match the given <paramref name="match"/> predicate from the selection.
    /// </summary>
    /// <param name="match">The predicate used to test each item key.</param>
    /// <returns>
    /// The number of items removed.
    /// </returns>
    int Remove(Predicate<TGridItemKey> match);

    /// <summary>
    /// Clears the selection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> is part of the selection.
    /// </summary>
    /// <param name="item">The item key to locate.</param>
    /// <returns>
    /// <see langword="true"/> if the item is contained in the selection; otherwise <see langword="false"/>.
    /// </returns>
    bool Contains(TGridItemKey item);
}
