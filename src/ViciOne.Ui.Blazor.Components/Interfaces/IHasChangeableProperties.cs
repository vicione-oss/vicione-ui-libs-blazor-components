using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes an instance with changeable properties
/// </summary>
public interface IHasChangeableProperties
{
    /// <summary>
    /// Raised when one or more properties have changed
    /// </summary>
    event Action<PropertiesChangedEventArgs>? PropertiesChanged;
}
