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
    /// Called when a drag enters the dropzone.
    /// </summary>
    Task DragEnterAsync(IDraggable draggable);

    /// <summary>
    /// Called when a drag leaves the dropzone.
    /// </summary>
    Task DragLeaveAsync();

    /// <summary>
    /// Called when a drag ends (such as releasing the pointer).
    /// This method is called before <see cref="DragDroppedAsync(IDraggable, double, double)"/>.
    /// </summary>
    /// <param name="draggable">Component that was dragged</param>
    /// <param name="x">Horizontal coordinate where the drag ended relative to the viewport</param>
    /// <param name="y">Vertical coordinate where the drag ended relative to the viewport</param>
    Task DragEndAsync(IDraggable draggable, double x, double y);

    /// <summary>
    /// Called when a <paramref name="draggable"/> was dropped on this dropzone.
    /// </summary>
    /// <param name="draggable">Component that has been dropped</param>
    /// <param name="x">Horizontal coordinate relative to the bounding client rectangle of this dropzone</param>
    /// <param name="y">Vertical coordinate relative to the bounding client rectangle of this dropzone</param>
    Task DragDroppedAsync(IDraggable draggable, double x, double y);
}
