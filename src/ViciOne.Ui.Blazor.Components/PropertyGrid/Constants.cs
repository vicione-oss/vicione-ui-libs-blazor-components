using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid;

/// <summary>
/// Constants used in the PropertyGrid namespace
/// </summary>
public sealed class Constants
{
    /// <summary>
    /// Supported boolean types for generic parameter <b><c>TPropertyValue</c></b> of <see cref="PropertyDescriptor{TInstance, TPropertyValue}"/>
    /// </summary>
    public static readonly Type[] SupportedBooleanPropertyValueTypes =
    [
        typeof(bool),
        typeof(bool?)
    ];
}
