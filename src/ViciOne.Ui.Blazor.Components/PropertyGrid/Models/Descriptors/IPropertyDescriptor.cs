using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <summary>
/// Describes a property
/// </summary>
public interface IPropertyDescriptor
{
    /// <summary>
    /// Unique name for the property
    /// </summary>
    /// <remarks>
    /// The name is used in various parts of the component like for determining common properties
    /// across multiple instances set via <see cref="IPropertyGridController{TContext}.SetInstances(IEnumerable{object}, TContext)"/>.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Text used to categorize the property
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Display name of the property
    /// </summary>
    /// <remarks>
    /// When not set, <see cref="Name"/> is used as display name.
    /// </remarks>
    string? DisplayName { get; }

    /// <summary>
    /// Description of the property
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Text displayed when the mouse cursor hovers the information tooltip icon
    /// </summary>
    string? InformationTooltip { get; }

    /// <summary>
    /// Type that implements the property
    /// </summary>
    Type TargetType { get; }

    /// <summary>
    /// Type of the property value
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Holds <see langword="true"/> when an UI option should be rendered that allows setting the
    /// property to <see langword="null"/>, otherwise <see langword="false"/>.
    /// </summary>
    bool CanBeSetToNull { get; }

    /// <summary>
    /// List of property descriptors that this property descriptor depends on.
    /// </summary>
    /// <remarks>
    /// When this property descriptor depends on another property descriptor, then changes to the
    /// property associated with the other property descriptor will cause an update of the
    /// property entry displayed in the property grid, that is associated with this property descriptor.
    /// </remarks>
    IEnumerable<IPropertyDescriptor>? DependsOn { get; }
}
