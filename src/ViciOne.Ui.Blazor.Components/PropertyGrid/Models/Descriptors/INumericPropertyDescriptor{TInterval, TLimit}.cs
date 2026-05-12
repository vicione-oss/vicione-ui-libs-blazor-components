namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

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
