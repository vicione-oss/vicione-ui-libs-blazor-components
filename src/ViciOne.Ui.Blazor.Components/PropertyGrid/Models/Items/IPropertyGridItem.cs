using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

/// <summary>
/// Represents an abstraction to read from and write to a set of related properties exposed in a property grid.
/// Implementations adapt one or more underlying properties to a uniform API used by the property grid UI.
/// </summary>
public interface IPropertyGridItem
{
    /// <summary>
    /// Unique key/name identifying the underlying property or group of properties.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Text used to group the property in the property grid (category header).
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Human-readable label shown in the property grid. When not set, the <see cref="Name"/> is typically used.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The type of the values accepted by the underlying properties.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// A short description of the property shown in documentation or tooling.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Text displayed in an information tooltip for the property.
    /// </summary>
    string? InformationTooltipText { get; }

    /// <summary>
    /// When <see langword="true"/>, the UI may render an option that allows setting the underlying property(ies) to <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// The null-setting UI is typically shown only when <see cref="Enabled"/> is <see langword="true"/> and
    /// <see cref="ReadOnly"/> is <see langword="false"/>.
    /// </remarks>
    bool CanBeSetToNull { get; }

    /// <summary>
    /// When <see langword="true"/>, the UI may render an option that allows resetting the underlying property(ies) to their default value(s).
    /// </summary>
    bool Resettable { get; }

    /// <summary>
    /// When <see langword="true"/>, the underlying property(ies) should be considered read-only and editors should be disabled.
    /// </summary>
    bool ReadOnly { get; }

    /// <summary>
    /// When <see langword="true"/>, editors for the property are enabled; otherwise editors should be disabled.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Holds <see langword="true"/> when the item should be visible, otherwise <see langword="false"/>.
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Message store used to collect validation, informational or error messages related to this item.
    /// Implementations update this store when validation or other checks produce messages.
    /// </summary>
    internal IPropertyGridMessageStore MessageStore { get; }

    /// <summary>
    /// Descriptors describing the underlying properties represented by this item.
    /// </summary>
    IEnumerable<IPropertyDescriptor> PropertyDescriptors { get; }

    /// <summary>
    /// Reset the underlying property(ies) to their default value(s) when possible.
    /// </summary>
    /// <returns><see langword="true"/> when any underlying property was successfully reset; otherwise <see langword="false"/>.</returns>
    internal bool ResetValue();

    /// <summary>
    /// Set the underlying property(ies) to <see langword="null"/> when supported.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the operation succeeded (the property(ies) were set to <see langword="null"/>),
    /// otherwise <see langword="false"/>.
    /// </returns>
    internal bool SetNull();
}

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
