namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

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
