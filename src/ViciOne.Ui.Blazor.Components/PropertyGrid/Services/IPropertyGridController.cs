using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Controller for a property grid
/// </summary>
public interface IPropertyGridController
{
    /// <summary>
    /// State of the property grid associated with the controller
    /// </summary>
    internal IPropertyGridState State { get; }

    /// <summary>
    /// Events exposed by the property grid associated with the controller
    /// </summary>
    internal IPropertyGridEvents Events { get; }

    /// <summary>
    /// Raised when <see cref="UpdateProperty(string)"/> was called
    /// </summary>
    internal event Action<PropertyGridControllerUpdatePropertyRequestedEventArgs>? UpdatePropertyRequested;

    /// <summary>
    /// Raised when <see cref="FocusProperty(string)"/> was called
    /// </summary>
    internal event Action<PropertyGridControllerFocusPropertyRequestedEventArgs>? FocusPropertyRequested;

    /// <summary>
    /// Updates the property with the given <paramref name="name"/>.
    /// </summary>
    /// <remarks>
    /// The given <paramref name="name"/> must match any of the names defined via <see cref="IPropertyDescriptor.Name"/>.
    /// </remarks>
    void UpdateProperty(string name);

    /// <summary>
    /// Updates dependents of the given <paramref name="propertyGridItem"/>
    /// </summary>
    internal void UpdateDependents(IPropertyGridItem propertyGridItem);

    /// <summary>
    /// Sets focus to the property with the given <paramref name="name"/>.
    /// </summary>
    /// <remarks>
    /// The given <paramref name="name"/> must match any of the names defined via <see cref="IPropertyDescriptor.Name"/>.
    /// </remarks>
    void FocusProperty(string name);

    /// <summary>
    /// Show context menu for the given <paramref name="context"/>
    /// </summary>
    internal Task ShowContextMenuAsync(PropertyEntryContextMenuContext context);
}
