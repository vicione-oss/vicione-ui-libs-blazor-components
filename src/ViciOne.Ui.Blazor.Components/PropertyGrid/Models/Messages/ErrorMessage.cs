namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

/// <summary>
/// Error message displayed in a property grid
/// </summary>
public class ErrorMessage : IMessage
{
    /// <inheritdoc/>
    public required string Text { get; init; }
}
