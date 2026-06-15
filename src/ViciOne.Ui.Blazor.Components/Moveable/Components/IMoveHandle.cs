using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Moveable.Components;

/// <summary>
/// A component that acts as a move handle in a move interaction.
/// </summary>
public interface IMoveHandle
{
    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();
}
