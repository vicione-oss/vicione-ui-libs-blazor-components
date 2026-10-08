namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

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
    /// Throw <see cref="Exceptions.ResetValueException"/> to indicate reset failures.
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
    /// <para>
    /// A hidden property is not validated. When it is refreshed while hidden, its messages are removed from the
    /// <see cref="Services.IPropertyGridMessageStore"/>. A property is refreshed when a property listed in
    /// <see cref="IPropertyDescriptor.DependsOn"/> is changed in the property grid, or when
    /// <see cref="Services.IPropertyGridController.UpdateProperty"/> is called for this property. List the
    /// properties the predicate reads in <see cref="IPropertyDescriptor.DependsOn"/>, so the property gets validated
    /// once it becomes visible.
    /// </para>
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
