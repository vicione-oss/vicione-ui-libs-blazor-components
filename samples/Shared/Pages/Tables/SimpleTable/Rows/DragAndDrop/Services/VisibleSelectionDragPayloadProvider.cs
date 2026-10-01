using Shared.Pages.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Services;

/// <summary>
/// Carries only what the user can see: a selected row the filter hides must not ride along on the drag.
/// Replace with the rule the consuming application needs.
/// </summary>
internal sealed class VisibleSelectionDragPayloadProvider(VisibleSelectionStore visibleSelection)
    : ITableDragPayloadProvider<ExampleTableItem>
{
    public IReadOnlyList<ExampleTableItem> GetPayload(ExampleTableItem draggedRow,
        IReadOnlyList<ExampleTableItem> resolvedPayload)
            => [.. resolvedPayload.Where(visibleSelection.Contains)];
}
