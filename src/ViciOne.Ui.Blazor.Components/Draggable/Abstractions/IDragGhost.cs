using ViciOne.Ui.Blazor.Components.Draggable.Models;

namespace ViciOne.Ui.Blazor.Components.Draggable.Abstractions;

/// <summary>
/// A drag ghost implementation backed by a JavaScript module to handle client-side aspects.
/// </summary>
public interface IDragGhost
{
    /// <summary>
    /// Gets the JavaScript module descriptor.
    /// </summary>
    DragGhostJsModuleDescriptor GetJsModule();
}
