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
