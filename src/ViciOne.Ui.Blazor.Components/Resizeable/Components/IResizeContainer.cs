using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Components;

/// <summary>
/// A component acting as the container in which a resize interaction can take place.
/// </summary>
public interface IResizeContainer
{
    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();
}
