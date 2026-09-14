using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <summary>
/// Handles a drop of a draggable on the specified <typeparamref name="TTarget"/>.
/// </summary>
public interface IDropHandler<TTarget>
{
    /// <summary>
    /// Called when <paramref name="draggable"/> is dropped on <paramref name="target"/> at the given coordinates.
    /// </summary>
    Task DragDroppedAsync(IDraggable draggable, double x, double y, TTarget target);
}
