using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Arguments for <see cref="IPropertyGridEvents.PropertyChanged"/>
/// </summary>
public sealed class PropertyGridPropertyChangedEventArgs : EventArgs
{
    /// <summary>
    /// Instance that raised the associated <see cref="IPropertyGridEvents.PropertyChanged"/> event.
    /// </summary>
    public required IPropertyGridEvents Sender { get; init; }

    /// <summary>
    /// Item associated with the changed property.
    /// </summary>
    public required IPropertyGridItem Item { get; init; }
}
