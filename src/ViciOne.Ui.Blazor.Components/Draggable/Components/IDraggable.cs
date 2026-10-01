using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

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

    /// <summary>
    /// Invoked when a drag of this draggable starts, and awaited before <see cref="IDragInteraction.DragStart"/>
    /// is raised. Settle here whatever the drag's dropzones decide on — typically the payload the drag carries.
    /// </summary>
    /// <remarks>
    /// The drag ghost and the pointer capture are already live when this runs, so awaiting here delays only when
    /// the dropzones become known, never the drag itself. Does nothing unless implemented.
    /// </remarks>
    Task PrepareDragStartAsync()
        => Task.CompletedTask;
}
