namespace ViciOne.Ui.Blazor.Components.Draggable.Abstractions;

/// <summary>
/// Opt-in lifecycle contract for a drag ghost that wants to be notified when a drag starts.
/// </summary>
/// <remarks>
/// A drag ghost deriving from <see cref="Components.DragGhostBase"/> implements this interface to receive the
/// drag-start callback. When implemented, the JavaScript side forwards the drag start to
/// <see cref="DragStartAsync"/>; when it is not implemented, no round-trip is made.
/// </remarks>
public interface IDragStartListener
{
    /// <summary>
    /// Invoked when a drag starts. The callback must not block the drag — the drag ghost is
    /// purely cosmetic and the drag proceeds regardless of the returned task.
    /// </summary>
    /// <remarks>
    /// When this callback mutates state the drag ghost content depends on, call
    /// <see cref="Components.DragGhostBase.RenderContentAsync"/> to re-render it. The JavaScript side
    /// then swaps the freshly rendered content into the drag ghost.
    /// </remarks>
    Task DragStartAsync();
}
