using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Services;

/// <summary>
/// Interface for a resize interaction
/// </summary>
public interface IResizeInteraction
{
    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the resizable when the resize interaction has been started.
    /// </summary>
    /// <remarks>
    /// The text will be removed from the attribute again, when the resize interaction is finished.
    /// </remarks>
    string StartedCssClass { get; }

    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the resizable when the resize interaction has transitioned from the started to the ongoing state.
    /// The transition happens when the first mouse move event is received.
    /// </summary>
    /// <remarks>
    /// The text will be removed from the attribute again, when the resize interaction is finished.
    /// </remarks>
    string OngoingCssClass { get; }

    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// of the resizable when the resize interaction has transitioned from ongoing to the ended state.
    /// That is normally the case when the mouse button was released <b>and</b> the mouse was moved.
    /// </summary>
    string EndedCssClass { get; }

    /// <summary>
    /// Attaches the interaction with the optionally specified <paramref name="pointerCaptureBehaviors"/>
    /// to the specified <paramref name="resizeable"/>.
    /// </summary>
    Task AttachAsync(IResizeable resizeable, IEnumerable<IPointerCaptureBehavior>? pointerCaptureBehaviors = null);

    /// <summary>
    /// Removes the interaction from the specified <paramref name="resizeable"/>.
    /// </summary>
    Task RemoveAsync(IResizeable resizeable);

    /// <summary>
    /// Adds the specified <paramref name="pointerCaptureBehavior"/> to the specified <paramref name="resizeable"/>.
    /// </summary>
    /// <remarks>
    /// The resize interaction must already be <see cref="AttachAsync(IResizeable, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task AddPointerCaptureBehaviorAsync(IResizeable resizeable, IPointerCaptureBehavior pointerCaptureBehavior);

    /// <summary>
    /// Removes the specified <paramref name="pointerCaptureBehavior"/> from the specified <paramref name="resizeable"/>.
    /// </summary>
    /// <remarks>
    /// The resize interaction must already be <see cref="AttachAsync(IResizeable, IEnumerable{IPointerCaptureBehavior}?)">attached</see>
    /// when calling this method, otherwise the method does nothing.
    /// </remarks>
    Task RemovePointerCaptureBehaviorAsync(IResizeable resizeable, IPointerCaptureBehavior pointerCaptureBehavior);
}
