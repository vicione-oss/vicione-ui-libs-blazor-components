namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridControllerFocusPropertyRequestedEventArgs : EventArgs
{
    public required string Name { get; set; }
}
