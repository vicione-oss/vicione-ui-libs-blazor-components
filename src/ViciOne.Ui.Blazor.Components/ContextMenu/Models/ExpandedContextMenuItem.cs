using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

/// <summary>
/// Context menu item whose child context menu is open or about to open, as tracked by
/// <see cref="ContextMenuCoordinator"/>
/// </summary>
/// <remarks>
/// Disposing it collapses the item, which stops a child context menu that is still about to open.
/// </remarks>
internal sealed class ExpandedContextMenuItem : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IContextMenuItem Item { get; }

    /// <summary>
    /// Canceled as soon as the item is collapsed
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Direction in which the mouse pointer leaves the item to reach the shown child context menu, or null while
    /// the child context menu is not shown yet
    /// </summary>
    public MouseLeaveDirection? DirectionTowardsChildContextMenu { get; set; }

    public bool IsCollapsed => _cancellationTokenSource.IsCancellationRequested;

    public ExpandedContextMenuItem(IContextMenuItem item)
    {
        Item = item;
        CancellationToken = _cancellationTokenSource.Token;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
