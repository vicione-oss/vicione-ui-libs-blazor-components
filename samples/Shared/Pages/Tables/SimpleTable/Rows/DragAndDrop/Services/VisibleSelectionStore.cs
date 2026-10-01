using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Services;

// The consumer's own mirror of what VisibleSelectionChanged last reported. It lives outside the page so the
// drag payload provider can read it without the page handing it over.
internal sealed class VisibleSelectionStore
{
    private HashSet<ExampleTableItem> _visibleSelection = [];

    public int Count => _visibleSelection.Count;

    public void Store(IReadOnlyList<ExampleTableItem> visibleSelection)
        => _visibleSelection = [.. visibleSelection];

    public bool Contains(ExampleTableItem item)
        => _visibleSelection.Contains(item);
}
