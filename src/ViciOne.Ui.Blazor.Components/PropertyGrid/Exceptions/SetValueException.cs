using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;

/// <summary>
/// Represents an error that occured during execution of <see cref="IPropertyDescriptor{TInstance, TPropertyValue}.SetValue"/>
/// </summary>
public sealed class SetValueException : Exception
{
    /// <summary>
    /// Property descriptor associated with the exception
    /// </summary>
    public IPropertyDescriptor? PropertyDescriptor { get; }

    /// <summary>
    /// Instance associated with the exception
    /// </summary>
    public object? Instance { get; }

    /// <summary>
    /// Value passed to <see cref="IPropertyDescriptor{TInstance, TPropertyValue}.SetValue"/>
    /// </summary>
    public object? Value { get; }

    /// <inheritdoc/>
    public SetValueException(string message, object instance, object? value) : base(message)
    {
        Instance = instance;
        Value = value;
    }

    /// <inheritdoc/>
    public SetValueException(string message, object instance, object? value, Exception innerException) : base(message, innerException)
    {
        Instance = instance;
        Value = value;
    }

    /// <inheritdoc/>
    internal SetValueException()
    {
    }

    /// <inheritdoc/>
    internal SetValueException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    internal SetValueException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    internal SetValueException(string message, IPropertyDescriptor propertyDescriptor, object instance, object? value, Exception innerException)
        : base(message, innerException)
    {
        PropertyDescriptor = propertyDescriptor;
        Instance = instance;
        Value = value;
    }
}
