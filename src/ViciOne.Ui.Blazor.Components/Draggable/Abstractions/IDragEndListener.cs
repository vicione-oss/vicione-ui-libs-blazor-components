namespace ViciOne.Ui.Blazor.Components.Draggable.Abstractions;

/// <summary>
/// Opt-in lifecycle contract for a drag ghost that wants to be notified when a drag ends.
/// </summary>
/// <remarks>
/// A drag ghost deriving from <see cref="Components.DragGhostBase"/> implements this interface to receive the
/// drag-end callback. When implemented, the JavaScript side forwards the drag end to
/// <see cref="DragEndAsync"/>; when it is not implemented, no round-trip is made.
/// </remarks>
public interface IDragEndListener
{
    /// <summary>
    /// Invoked when a drag ends. The callback must not block the drag — the drag ghost is
    /// purely cosmetic and the drag proceeds regardless of the returned task.
    /// </summary>
    /// <remarks>
    /// When this callback mutates state the drag ghost content depends on, call
    /// <see cref="Components.DragGhostBase.RenderContentAsync"/> to re-render it, so the off-screen content
    /// reflects the reverted state for the next drag.
    /// </remarks>
    Task DragEndAsync();
}
