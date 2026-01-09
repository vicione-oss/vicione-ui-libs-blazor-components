using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Arguments for changed notification of <see cref="IPropertyGridMessageStore"/>
/// </summary>
public sealed class PropertyGridMessageStoreChangedEventArgs : EventArgs
{
    /// <summary>
    /// Instance that raised the associated <see cref="IPropertyGridMessageStore.Changed"/> event
    /// </summary>
    public required IPropertyGridMessageStore Sender { get; init; }

    /// <summary>
    /// Items for which messages have been added to the message store
    /// </summary>
    public required IReadOnlySet<IPropertyGridItem> Added { get; init; }


    /// <summary>
    /// Items for which messages have been removed from the message store
    /// </summary>
    public required IReadOnlySet<IPropertyGridItem> Removed { get; init; }
}
