using ViciOne.Ui.Blazor.Components.LoadingSpinner.Models;

namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Factories;

/// <summary>
/// A factory to create <see cref="TimedMessage"/>s.
/// </summary>
public static class TimedMessageFactory
{
    /// <summary>
    /// Creates a new <see cref="TimedMessage"/> acting as a gap between two
    /// others.
    /// </summary>
    ///
    /// <param name="duration">
    /// The amount of seconds the gap shall last for.
    /// </param>
    ///
    /// <returns>
    /// The newly created gap-message.
    /// </returns>
    public static TimedMessage CreateGap(int duration)
        => new()
        {
            DisplayDuration = duration,
            Message = string.Empty,
        };
}
