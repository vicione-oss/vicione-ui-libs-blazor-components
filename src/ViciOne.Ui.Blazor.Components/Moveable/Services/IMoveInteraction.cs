using ViciOne.Ui.Blazor.Components.Moveable.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Moveable.Services;

/// <summary>
/// Interface for a move interaction
/// </summary>
public interface IMoveInteraction
{
    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// when a move interaction has been started.
    /// </summary>
    string StartedCssClass { get; }

    /// <summary>
    /// Gets the text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// when a move interaction has ended.
    /// </summary>
    string EndedCssClass { get; }

    /// <summary>
    /// Attaches the interaction to the given <paramref name="moveable"/>.
    /// </summary>
    Task AttachAsync(IMoveable moveable);

    /// <summary>
    /// Removes the interaction from the given <paramref name="moveable"/>.
    /// </summary>
    Task RemoveAsync(IMoveable moveable);
}
