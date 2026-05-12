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
