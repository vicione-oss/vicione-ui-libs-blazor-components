using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.Moveable.Services;

/// <summary>
/// Interface for a move interaction
/// </summary>
public interface IMoveInteraction
{
    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// when a move interaction has been started. The text will be removed from the attribute again,
    /// when finished.
    /// </summary>
    string StartedCssClass { get; }

    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// when a move interaction has ended.
    /// </summary>
    string EndedCssClass { get; }

    /// <summary>
    /// Attaches the interaction with the optionally specified <paramref name="pointerCaptureBehaviors"/>
    /// to the specified <paramref name="moveable"/>.
    /// </summary>
    Task AttachAsync(IMoveable moveable, IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null);

    /// <summary>
    /// Removes the interaction from the specified <paramref name="moveable"/>.
    /// </summary>
    Task RemoveAsync(IMoveable moveable);

    /// <summary>
    /// Adds the specified <paramref name="pointerCaptureBehavior"/> to the specified <paramref name="moveable"/>.
    /// </summary>
    /// <remarks>
    /// The move interaction must already be <see cref="AttachAsync(IMoveable, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task AddPointerCaptureBehaviorAsync(IMoveable moveable, IPointerCaptureBehavior pointerCaptureBehavior);

    /// <summary>
    /// Removes the specified <paramref name="pointerCaptureBehavior"/> from the specified <paramref name="moveable"/>.
    /// </summary>
    /// <remarks>
    /// The move interaction must already be <see cref="AttachAsync(IMoveable, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task RemovePointerCaptureBehaviorAsync(IMoveable moveable, IPointerCaptureBehavior pointerCaptureBehavior);
}
