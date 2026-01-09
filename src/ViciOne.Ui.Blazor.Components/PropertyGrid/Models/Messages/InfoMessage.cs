namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

/// <summary>
/// Info message displayed in a property grid
/// </summary>
public class InfoMessage : IMessage
{
    /// <inheritdoc/>
    public required string Text { get; init; }
}
