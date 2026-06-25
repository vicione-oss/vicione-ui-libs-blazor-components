using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Enums;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Components;

/// <summary>
/// A component that acts as a resize handle in a resize interaction.
/// </summary>
public interface IResizeHandle
{
    /// <summary>
    /// Gets the position of the resize handle.
    /// </summary>
    ResizeHandlePosition Position { get; }

    /// <summary>
    /// Gets the element reference of the component for use in connection with JS interop.
    /// </summary>
    ElementReference GetElementReference();
}
