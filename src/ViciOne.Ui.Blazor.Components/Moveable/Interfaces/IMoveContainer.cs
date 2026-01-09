using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Moveable.Interfaces;

/// <summary>
/// A component acting as the container in which a move interaction can take place.
/// </summary>
public interface IMoveContainer
{
    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();
}
