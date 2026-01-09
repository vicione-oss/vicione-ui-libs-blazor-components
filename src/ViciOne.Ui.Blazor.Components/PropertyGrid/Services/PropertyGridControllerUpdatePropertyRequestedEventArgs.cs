using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridControllerUpdatePropertyRequestedEventArgs : EventArgs
{
    public required IReadOnlySet<IPropertyGridItem> Targets { get; set; }
}
