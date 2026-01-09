using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

/// <summary>
/// Describes an interation of an operation targeting a property implemented by instances of a specific type
/// </summary>
internal sealed class PropertyOperationIteration
{
    /// <summary>
    /// Actual type of the instances stored in <see cref="Instances"/>
    /// </summary>
    public required Type InstanceType { get; init; }

    /// <summary>
    /// Actual type of the property implemented by <see cref="InstanceType"/>
    /// </summary>
    public required Type PropertyValueType { get; init; }

    /// <summary>
    /// Boxed <see cref="IPropertyDescriptor{TInstance, TPropertyValue}"/> where
    /// TInstance is <see cref="InstanceType"/>
    /// </summary>
    public required object PropertyDescriptor { get; init; }

    /// <summary>
    /// Instances on which the property operation is executed
    /// </summary>
    public required IReadOnlyList<object> Instances { get; init; }
}
