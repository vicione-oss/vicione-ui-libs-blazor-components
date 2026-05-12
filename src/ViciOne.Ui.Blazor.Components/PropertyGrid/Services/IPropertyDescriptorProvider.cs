namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// Provider for property descriptors
/// </summary>
public interface IPropertyDescriptorProvider
{
    /// <summary>
    /// Type of the instances targeted by the property descriptors provided.
    /// </summary>
    Type GetTargetInstanceType();
}
