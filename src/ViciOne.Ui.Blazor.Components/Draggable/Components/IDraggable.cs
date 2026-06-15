using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Components;

/// <summary>
/// A component that can be dragged.
/// </summary>
public interface IDraggable
{
    /// <summary>
    /// <see langword="true"/> when dragging is allowed, otherwise <see langword="false"/>.
    /// </summary>
    bool Draggable { get; }

    /// <summary>
    /// Gets the element reference of this component.
    /// </summary>
    ElementReference GetElementReference();
}
