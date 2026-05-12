using ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

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
