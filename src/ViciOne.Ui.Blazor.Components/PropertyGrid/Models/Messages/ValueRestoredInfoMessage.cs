namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

internal sealed class ValueRestoredInfoMessage : InfoMessage
{
    public required IEnumerable<string> Reasons { get; init; }
}
