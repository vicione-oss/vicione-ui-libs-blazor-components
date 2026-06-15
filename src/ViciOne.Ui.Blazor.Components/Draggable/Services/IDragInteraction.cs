using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <summary>
/// Manages drag interactions between <see cref="IDraggable"/> components and <see cref="IDropzone"/> components.
/// </summary>
public interface IDragInteraction
{
    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the draggable when a drag interaction has been started.
    /// </summary>
    /// <remarks>
    /// The text will be removed from the attribute again, when the drag interaction is finished.
    /// </remarks>
    string StartedCssClass { get; }

    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the draggable when a drag interaction has ended.
    /// </summary>
    string EndedCssClass { get; }

    /// <summary>
    /// Raised when a drag of a <see cref="IDraggable"/> is started.
    /// </summary>
    event EventHandler<DragStartEventArgs>? DragStart;

    /// <summary>
    /// <para>
    ///     Attaches the interaction to the given <paramref name="draggable"/>.
    /// </para>
    /// <para>
    ///     An optional <paramref name="modifierKey"/> can be passed to allow the interaction
    ///     only when that key is held.
    /// </para>
    /// <para>
    ///     Optional <paramref name="pointerCaptureBehaviors"/> are applied to the drag clone
    ///     while dragging — for example to snap movement to a grid.
    /// </para>
    /// </summary>
    Task AttachAsync(IDraggable draggable, ModifierKey? modifierKey = null,
        IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null);

    /// <summary>
    /// Removes the interaction from the given <paramref name="draggable"/>.
    /// </summary>
    Task RemoveAsync(IDraggable draggable);

    /// <summary>
    /// Adds the specified <paramref name="pointerCaptureBehavior"/> to the specified <paramref name="draggable"/>.
    /// </summary>
    /// <remarks>
    /// The drag interaction must already be <see cref="AttachAsync(IDraggable, ModifierKey?, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task AddPointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior);

    /// <summary>
    /// Removes the specified <paramref name="pointerCaptureBehavior"/> from the specified <paramref name="draggable"/>.
    /// </summary>
    /// <remarks>
    /// The drag interaction must already be <see cref="AttachAsync(IDraggable, ModifierKey?, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task RemovePointerCaptureBehaviorAsync(IDraggable draggable, IPointerCaptureBehavior pointerCaptureBehavior);
}
