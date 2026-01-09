namespace ViciOne.Ui.Blazor.Components.Grid.Services;

/// <summary>
/// Arguments for <see cref="IGridItemSelection{TSelectable}.Changed"/> event
/// </summary>
public sealed class GridItemSelectionChangedEventArgs<TGridItemKey> : EventArgs
{
    /// <summary>
    /// Instance that raised the associated <see cref="IGridItemSelection{TSelectable}.Changed"/> event
    /// </summary>
    public required IGridItemSelection<TGridItemKey> Sender { get; init; }

    /// <summary>
    /// Items added to <see cref="Sender"/>
    /// </summary>
    public required IReadOnlySet<TGridItemKey> ItemsAdded { get; init; }

    /// <summary>
    /// Items removed from <see cref="Sender"/>
    /// </summary>
    public required IReadOnlySet<TGridItemKey> ItemsRemoved { get; init; }
}
