using ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

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

/// <summary>
/// Describes a property of <typeparamref name="TInstance"/>
/// </summary>
public interface IPropertyDescriptor<TInstance> : IPropertyDescriptor
{
    /// <summary>
    /// Optional predicate evaluated for a given instance to determine whether this descriptor
    /// should participate in property processing for that instance.
    /// </summary>
    /// <remarks>
    /// Returning <see langword="true"/> means the descriptor is considered; returning <see langword="false"/>
    /// excludes the descriptor from further processing (for example, from display or from key generation).
    /// This predicate is called early in processing to avoid expensive operations when a descriptor
    /// does not apply to a specific instance.
    /// </remarks>
    Func<TInstance, bool>? ConsiderPredicate { get; }

    /// <summary>
    /// Action that resets the property on the provided instance to its reset/default state.
    /// </summary>
    /// <remarks>
    /// Implementations should typically reset the property to the value returned by
    /// <see cref="IPropertyDescriptor{TInstance, TPropertyValue}.GetDefaultValue"/> when available.
    /// Throw <see cref="ResetValueException"/> to indicate reset failures.
    /// If this property is <see langword="null"/>, the property is considered not resettable.
    /// </remarks>
    Action<TInstance>? ResetValue { get; }

    /// <summary>
    /// Optional predicate that determines whether the property should be visible for the given instance.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the property is considered visible. When provided, return
    /// <see langword="true"/> to show the property and <see langword="false"/> to hide it.
    /// Use this to implement conditional visibility based on instance state.
    /// </remarks>
    Func<TInstance, bool>? Visible { get; }

    /// <summary>
    /// Optional predicate that determines whether the property is considered resettable for the given instance.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, resettable behavior is not enforced by this predicate and
    /// the presence of a non-<see langword="null"/> <see cref="ResetValue"/> typically determines reset capability.
    /// Return <see langword="true"/> when the property may be reset; return <see langword="false"/> otherwise.
    /// </remarks>
    Func<TInstance, bool>? Resettable { get; }

    /// <summary>
    /// Optional predicate that determines whether the property is read-only for the given instance.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the property is treated as not read-only (editable) by default.
    /// When provided, return <see langword="true"/> to mark the property as read-only, or <see langword="false"/>
    /// to allow editing. Use this to implement instance-specific read-only logic.
    /// </remarks>
    Func<TInstance, bool>? ReadOnly { get; }

    /// <summary>
    /// Optional predicate that determines whether the property's editor/input should be enabled for the given instance.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the editor input is considered enabled by default.
    /// When provided, return <see langword="true"/> to enable the input, or <see langword="false"/> to disable it.
    /// This differs from <see cref="ReadOnly"/> in that the editor may remain visible but disabled.
    /// </remarks>
    Func<TInstance, bool>? Enabled { get; }
}

/// <summary>
/// Describes a property of <typeparamref name="TInstance"/> whereas the property value is <typeparamref name="TPropertyValue"/>
/// </summary>
public interface IPropertyDescriptor<TInstance, TPropertyValue> : IPropertyDescriptor<TInstance>
{
    /// <summary>
    /// Validators used to validate the value returned by <see cref="GetValue"/> or to validate the value
    /// to be passed to <see cref="SetValue"/>.
    /// </summary>
    IEnumerable<IPropertyValueValidator<TPropertyValue>>? ValueValidators { get; }

    /// <summary>
    /// Returns the default value of the property.
    /// </summary>
    /// <remarks>
    /// When not assigned, the property is considered to not have a default value,
    /// hence <see cref="HasValueDifferentFromDefaultValue"/> takes no effect.
    /// </remarks>
    Func<TInstance, TPropertyValue>? GetDefaultValue { get; }

    /// <summary>
    /// Action that assigns the given value (second parameter) to the property of the given instance (first parameter).
    /// </summary>
    /// <remarks>
    /// Throw <see cref="SetValueException"/> to communicate errors.
    /// </remarks>
    Action<TInstance, TPropertyValue>? SetValue { get; }

    /// <summary>
    /// Function that returns the value of the property for the given instance.
    /// </summary>
    Func<TInstance, TPropertyValue> GetValue { get; }

    /// <summary>
    /// Function that returns <see langword="true"/> when the property has a value different to its default value
    /// passed as second parameter, otherwise <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// When assigned, this property overrides the default behavior of comparing the <see cref="GetValue">current value</see>
    /// and <see cref="GetDefaultValue">default value</see> via <see cref="IPropertyValueEqualityComparer"/> to determine
    /// whether the property has a value different from the default value.
    /// </remarks>
    Func<TInstance, TPropertyValue, bool>? HasValueDifferentFromDefaultValue { get; }
}
