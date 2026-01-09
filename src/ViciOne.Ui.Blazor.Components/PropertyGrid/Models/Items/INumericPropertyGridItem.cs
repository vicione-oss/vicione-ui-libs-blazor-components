namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

/// <summary>
/// Models an API to read from / write to a set of similar numeric properties.
/// </summary>
internal interface INumericPropertyGridItem : IPropertyGridItem
{
    /// <summary>
    /// True when only a multiple of the value returned by <see cref="GetIntervalBoxed"/> is allowed as input, otherwise false
    /// </summary>
    bool IsRasteredValue { get; }

    /// <summary>
    /// Interval that is used when an increment / decrement operation is executed on underlying properties
    /// </summary>
    object GetIntervalBoxed();

    /// <summary>
    /// Minimum value that is accepted by underlying properties
    /// </summary>
    object GetMinimumBoxed();

    /// <summary>
    /// Maximum value that is accepted by underlying properties
    /// </summary>
    object GetMaximumBoxed();
}

/// <inheritdoc/>
internal interface INumericPropertyGridItem<TInterval, TLimit> : INumericPropertyGridItem
    where TInterval : struct
    where TLimit : struct
{
    /// <summary>
    /// Interval that is used when an increment / decrement operation is executed on underlying properties
    /// </summary>
    TInterval Interval { get; }

    /// <summary>
    /// Minimum value that is accepted by underlying properties
    /// </summary>
    TLimit Minimum { get; }

    /// <summary>
    /// Maximum value that is accepted by underlying properties
    /// </summary>
    TLimit Maximum { get; }
}
