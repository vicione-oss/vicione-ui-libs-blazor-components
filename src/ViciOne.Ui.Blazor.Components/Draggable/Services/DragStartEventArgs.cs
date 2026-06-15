using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <summary>
/// Arguments for event <see cref="IDragInteraction.DragStart"/>.
/// </summary>
public sealed class DragStartEventArgs : EventArgs
{
    /// <summary>
    /// Draggable associated with the drag.
    /// </summary>
    public required IDraggable Draggable { get; init; }

    /// <summary>
    /// Drop zones interested in receiving callbacks for this drag.
    /// </summary>
    public IList<IDropzone> Dropzones { get; } = [];
}
