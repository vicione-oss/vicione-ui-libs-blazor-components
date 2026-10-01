using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Exposes the payload of a draggable table row, allowing drop targets to read every item the drag
/// carries from the <see cref="IDraggable"/> parameter in <see cref="IDropzone.DragDroppedAsync"/>.
/// </summary>
/// <remarks>
/// <para>With no <see cref="AdvancedTable.Components.AdvancedTable{TItem}.DragPayloadProvider"/> supplied, dragging a selected row
/// carries the whole selection and dragging an unselected row carries just that row.</para>
/// <para>A supplied provider decides the payload instead, and what it answers with is passed on unchanged —
/// including rows the table never resolved, or none at all. A provider that throws counts as one answering
/// with none.</para>
/// <para>A drag carrying no rows still starts, so a drop target has to handle an empty payload.</para>
/// <para>The payload is frozen at drag start, before <see cref="Draggable.Services.IDragInteraction.DragStart"/> is
/// raised, so a dropzone deciding there whether to take part already reads it. It stays readable after the drop;
/// before a row's first drag it reads as just that row's item.</para>
/// </remarks>
/// <typeparam name="TItem">The type of data represented by the row.</typeparam>
public interface IDraggableRowSet<out TItem>
{
    /// <summary>
    /// The ordered set of items this drag carries. Empty when the table's payload provider answered with no
    /// rows.
    /// </summary>
    IReadOnlyList<TItem> Items { get; }
}
