using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// General events exposed by a property grid
/// </summary>
public interface IPropertyGridEvents
{
    /// <summary>
    /// Raised when a property value was changed.
    /// </summary>
    event Action<PropertyGridPropertyChangedEventArgs>? PropertyChanged;

    /// <summary>
    /// Raised when the context menu visibility was changed.
    /// </summary>
    event Action<PropertyGridContextMenuVisibilityChangedEventArgs> ContextMenuVisibilityChanged;

    /// <summary>
    /// Notifies that a property value has changed.
    /// </summary>
    internal void NotifyPropertyChanged(IPropertyGridItem item);

    /// <summary>
    /// Notifies that the context menu visibility has changed.
    /// </summary>
    internal void NotifyContextMenuVisibilityChanged(bool visible);
}
