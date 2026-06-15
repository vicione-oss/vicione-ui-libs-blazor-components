using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Components;

/// <summary>
/// A component that accepts dropped draggables.
/// </summary>
public interface IDropzone
{
    /// <summary>
    /// Gets the element reference of this component.
    /// </summary>
    ElementReference GetElementReference();

    /// <summary>
    /// Called when a drag enters the drop zone.
    /// </summary>
    Task DragEnterAsync(IDraggable draggable);

    /// <summary>
    /// Called when a drag leaves the drop zone.
    /// </summary>
    Task DragLeaveAsync();

    /// <summary>
    /// Called when a drag ends (such as releasing a mouse button).
    /// This method is called before <see cref="DragDroppedAsync(IDraggable, double, double)"/>.
    /// </summary>
    /// <param name="draggable">Component that was dragged</param>
    /// <param name="x">Horizontal coordinate where the drag ended relative to the viewport</param>
    /// <param name="y">Vertical coordinate where the drag ended relative to the viewport</param>
    Task DragEndAsync(IDraggable draggable, double x, double y);

    /// <summary>
    /// Called when a <paramref name="draggable"/> was dropped on this drop zone.
    /// </summary>
    /// <param name="draggable">Component that has been dropped</param>
    /// <param name="x">Horizontal coordinate relative to the bounding client rectangle of this drop zone</param>
    /// <param name="y">Vertical coordinate relative to the bounding client rectangle of this drop zone</param>
    Task DragDroppedAsync(IDraggable draggable, double x, double y);
}
