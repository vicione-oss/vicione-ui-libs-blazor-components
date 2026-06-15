using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace Shared.Pages.Draggable.Components;

/// <summary>
/// Accepts all draggables on all dropzones.
/// Replace with logic to restrict which draggables can drop on which dropzones.
/// </summary>
internal sealed class SampleDropPolicy : IDropPolicy<IDropzone>
{
    public bool Accepts(IDraggable draggable, IDropzone target) => true;
}
