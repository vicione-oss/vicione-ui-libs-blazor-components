using Microsoft.AspNetCore.Components.Web;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static class KeyboardEventArgsExtensions
{
    /// <summary>
    /// Determines whether the pressed key represents the Enter key.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns>
    /// true if <paramref name="args"/>.Key is "Enter" or "NumpadEnter"; otherwise false.
    /// </returns>
    public static bool IsEnter(this KeyboardEventArgs args)
        => args.Key is "Enter" or "NumpadEnter";
}
