namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <summary>
/// Describes a numeric property
/// </summary>
public interface INumericPropertyDescriptor
{
    /// <summary>
    /// Interval type
    /// </summary>
    Type IntervalType { get; }

    /// <summary>
    /// Limit type
    /// </summary>
    Type LimitType { get; }

    /// <summary>
    /// True when only a multiple of <see cref="IntervalType"/> is allowed as input, otherwise false
    /// </summary>
    bool IsRasteredValue { get; }
}


/// <inheritdoc/>
public interface INumericPropertyDescriptor<TInterval, TLimit> : INumericPropertyDescriptor
    where TInterval : struct // ensures nullable types cannot be used as a null interval does not make sense in foresight of using a spin control
    where TLimit : struct // ensures nullable types cannot be used as a null limit does not make sense in foresight of using limits to avoid overflow exceptions
{
    /// <summary>
    /// Interval that is used when an increment / decrement operation is executed on the underlying property
    /// </summary>
    TInterval Interval { get; }

    /// <summary>
    /// Minimum value that is accepted by the underlying property
    /// </summary>
    TLimit Minimum { get; }

    /// <summary>
    /// Maximum value that is accepted by the underlying property
    /// </summary>
    TLimit Maximum { get; }
}

/// <inheritdoc cref="INumericPropertyDescriptor{TInterval, TLimit}"/>
public interface INumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>
    : IPropertyDescriptor<TInstance, TPropertyValue>, INumericPropertyDescriptor<TInterval, TLimit>
        where TInterval : struct
        where TLimit : struct;
