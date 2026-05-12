namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

/// <summary>
/// Extends <see cref="IPropertyGridItem"/> with a statically-typed API for property values.
/// </summary>
/// <typeparam name="TPropertyValue">The type of the property value exposed by this item.</typeparam>
public interface IPropertyGridItem<TPropertyValue> : IPropertyGridItem
{
    /// <summary>
    /// Equality comparer used to determine value equality for <typeparamref name="TPropertyValue"/>.
    /// Implementations use this comparer when deciding if values differ or to detect unified values across instances.
    /// </summary>
    internal IEqualityComparer<TPropertyValue> ValueEqualityComparer { get; }

    /// <summary>
    /// Reads a unified value across all underlying instances, if such a uniform value exists.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="ValueOf{TPropertyValue}"/> containing the unified value when all underlying properties
    /// have the same value; otherwise <see langword="null"/> to indicate no single unified value could be determined.
    /// </returns>
    internal ValueOf<TPropertyValue>? ReadUnifiedValue();

    /// <summary>
    /// Assigns the provided <paramref name="value"/> to all underlying property(ies).
    /// </summary>
    /// <param name="value">The value to set for the underlying property(ies).</param>
    /// <returns><see langword="true"/> when the assignment succeeded for all target(s); otherwise <see langword="false"/>.</returns>
    internal bool SetValue(TPropertyValue value);

    /// <summary>
    /// Validates the supplied <paramref name="value"/> for the underlying property(ies).
    /// When validation fails, implementations should update <see cref="IPropertyGridItem.MessageStore"/> with messages
    /// describing the failure.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true"/> when validation succeeds; otherwise <see langword="false"/>.</returns>
    internal bool Validate(TPropertyValue value);

    /// <summary>
    /// Determines whether the default value for the underlying property(ies) differs from the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to compare with the default value (use <see langword="null"/> when there is no unified value).</param>
    /// <returns><see langword="true"/> when the default value is different from <paramref name="value"/>; otherwise <see langword="false"/>.</returns>
    internal bool IsDefaultValueDifferentFrom(ValueOf<TPropertyValue>? value);
}
