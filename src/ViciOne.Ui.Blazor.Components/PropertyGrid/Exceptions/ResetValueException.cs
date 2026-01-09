using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;

/// <summary>
/// Represents errors that occur during execution of <see cref="IPropertyDescriptor{TInstance}.ResetValue"/>
/// </summary>
public sealed class ResetValueException : Exception
{
    /// <summary>
    /// Property descriptor associated with the exception
    /// </summary>
    public IPropertyDescriptor? PropertyDescriptor { get; }

    /// <summary>
    /// Instance associated with the exception
    /// </summary>
    public object? Instance { get; }

    /// <inheritdoc/>
    public ResetValueException(string message, object instance) : base(message)
        => Instance = instance;

    /// <inheritdoc/>
    public ResetValueException(string message, object instance, Exception innerException) : base(message, innerException)
        => Instance = instance;

    /// <inheritdoc/>
    internal ResetValueException()
    {
    }

    /// <inheritdoc/>
    internal ResetValueException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    internal ResetValueException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    internal ResetValueException(string message, IPropertyDescriptor propertyDescriptor, object instance, Exception innerException)
        : base(message, innerException)
    {
        PropertyDescriptor = propertyDescriptor;
        Instance = instance;
    }
}
