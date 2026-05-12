namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services.TypeDescriptors;

/// <summary>
/// Describes a numeric value type.
/// </summary>
public interface INumericValueTypeDescriptor
{
    /// <summary>
    /// Gets the underlying type.
    /// </summary>
    Type UnderlyingType { get; }
}
