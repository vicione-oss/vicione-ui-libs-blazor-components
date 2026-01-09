namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Models;

/// <summary>
/// Render context for a message
/// </summary>
public sealed class MessageRenderContext
{
    /// <summary>
    /// <see langword="true"/> when the message visible, otherwise <see langword="false"/>
    /// </summary>
    public bool Visible { get; init; }

    /// <summary>
    /// Messsage text
    /// </summary>
    public string? Text { get; init; }
}
