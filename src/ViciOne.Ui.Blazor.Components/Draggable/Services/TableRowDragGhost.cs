using ViciOne.Ui.Blazor.Components.Draggable.Models;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <inheritdoc cref="ITableRowDragGhost"/>
internal sealed class TableRowDragGhost : ITableRowDragGhost
{
    /// <inheritdoc/>
    public DragGhostJsModuleDescriptor GetJsModule()
        => new()
        {
            ModuleName =
                $"/_content/{typeof(TableRowDragGhost).Assembly.GetName().Name}/draggable/table-row-drag-ghost.js",
            CreateFunction = new() { Name = "createDragGhost" }
        };
}
