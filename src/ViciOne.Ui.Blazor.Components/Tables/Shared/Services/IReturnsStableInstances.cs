namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

/// <summary>
/// Marker for an <see cref="IItemsProvider{TItem}"/> that provably returns the same object instances
/// for the same logical rows across fetches. The table uses this internal handshake to decide whether
/// reference-based selection identity is safe when no <c>ItemIdSelector</c> is supplied.
/// </summary>
internal interface IReturnsStableInstances;
