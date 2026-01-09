namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <inheritdoc cref="INumericPropertyDescriptor{TInstance, TPropertyValue, TInterval, TLimit}"/>
public class NumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>
    : PropertyDescriptor<TInstance, TPropertyValue>, INumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>
        where TInterval : struct
        where TLimit : struct
{
    /// <inheritdoc/>
    public bool IsRasteredValue { get; set; }

    /// <inheritdoc/>
    public Type IntervalType => typeof(TInterval);

    /// <inheritdoc/>
    public required TInterval Interval { get; set; }

    /// <inheritdoc/>
    public Type LimitType => typeof(TLimit);

    /// <inheritdoc/>
    public required TLimit Minimum { get; set; }

    /// <inheritdoc/>
    public required TLimit Maximum { get; set; }
}
