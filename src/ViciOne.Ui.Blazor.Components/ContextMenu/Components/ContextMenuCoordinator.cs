using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Decides which child context menus of one root context menu are shown
/// </summary>
/// <remarks>
/// <para>
/// At most one item per context menu has its child context menu open or about to open, so all of them form a
/// single path from the root context menu downwards, where every item lives in the child context menu of the
/// item before it. Opening a child context menu cuts the path off behind the item's own context menu, which
/// closes the child context menus of its siblings and their descendants in the same step.
/// </para>
/// <para>
/// Every member must be called on the renderer's dispatcher. The path is changed before the first await, so a
/// call that runs while another one awaits the browser always finds a consistent path.
/// </para>
/// </remarks>
internal sealed class ContextMenuCoordinator(IContextMenu rootContextMenu, TimeSpan showChildContextMenuDelay)
    : IDisposable
{
    /// <summary>
    /// Time the mouse pointer has to rest on an item before its child context menu is shown
    /// </summary>
    public static readonly TimeSpan DefaultShowChildContextMenuDelay = TimeSpan.FromMilliseconds(250);

    private readonly List<ExpandedContextMenuItem> _expandedItems = [];

    /// <summary>
    /// Uppermost context menu in the logical tree of context menus
    /// </summary>
    public IContextMenu RootContextMenu { get; } = rootContextMenu;

    /// <summary>
    /// Collects the context menus a click must land in to keep them open
    /// </summary>
    /// <returns>Root context menu and child context menus that are currently visible</returns>
    public IReadOnlyList<IContextMenu> GetVisibleContextMenus()
    {
        var result = new List<IContextMenu>();

        if (RootContextMenu.Visible)
            result.Add(RootContextMenu);

        foreach (var expandedItem in _expandedItems)
        {
            if (expandedItem.Item.ChildContextMenu?.Visible == true)
                result.Add(expandedItem.Item.ChildContextMenu);
        }

        return result;
    }

    /// <summary>
    /// Shows the child context menu of <paramref name="item"/> once the delay given to the constructor has elapsed,
    /// and closes the child context menus of its siblings right away
    /// </summary>
    /// <remarks>
    /// Does nothing when the child context menu is already open or about to open, so hovering and clicking the
    /// same item do not restart the delay.
    /// </remarks>
    public async Task ExpandAsync(IContextMenuItem item)
    {
        if (item.ChildContextMenu is null || IndexOf(item) >= 0)
            return;

        int firstSiblingIndex;

        if (item.ParentContextMenu == RootContextMenu)
        {
            firstSiblingIndex = 0;
        }
        else
        {
            var parentIndex = _expandedItems.FindIndex(e => e.Item.ChildContextMenu == item.ParentContextMenu);

            // The item's own context menu has been closed in the meantime.
            if (parentIndex < 0)
                return;

            firstSiblingIndex = parentIndex + 1;
        }

        var collapsedItems = RemoveFrom(firstSiblingIndex);

        var expandedItem = new ExpandedContextMenuItem(item);
        _expandedItems.Add(expandedItem);

        await CollapseAsync(collapsedItems, closingContextMenu: null);

        if (expandedItem.IsCollapsed)
            return;

        await item.StartObserveMouseLeaveAsync();

        await ShowChildContextMenuDelayedAsync(expandedItem);
    }

    /// <summary>
    /// Called when the mouse pointer has left <paramref name="item"/> in <paramref name="direction"/>
    /// </summary>
    /// <remarks>
    /// Keeps the child context menu open only when it is already shown and the mouse pointer heads towards it.
    /// </remarks>
    public async Task MouseLeftItemAsync(IContextMenuItem item, MouseLeaveDirection direction)
    {
        var index = IndexOf(item);

        // The item was collapsed while its observation was still being started.
        if (index < 0)
        {
            await item.EndObserveMouseLeaveAsync();
            return;
        }

        if (_expandedItems[index].DirectionTowardsChildContextMenu == direction)
            return;

        await CollapseAsync(RemoveFrom(index), closingContextMenu: null);
    }

    /// <summary>
    /// Called by <paramref name="contextMenu"/> before it is shown, so child context menus opened for its previous
    /// position or item filter do not stay behind
    /// </summary>
    public async Task ContextMenuShowingAsync(IContextMenu contextMenu)
    {
        int firstCollapsedIndex;

        if (contextMenu == RootContextMenu)
        {
            firstCollapsedIndex = 0;
        }
        else
        {
            var index = _expandedItems.FindIndex(e => e.Item.ChildContextMenu == contextMenu);

            // Not opened by this coordinator, so no child context menus of it are open either.
            if (index < 0)
                return;

            firstCollapsedIndex = index + 1;
        }

        await CollapseAsync(RemoveFrom(firstCollapsedIndex), closingContextMenu: null);
    }

    /// <summary>
    /// Called by <paramref name="contextMenu"/> before it hides, so its child context menus close with it
    /// </summary>
    public async Task ContextMenuClosingAsync(IContextMenu contextMenu)
    {
        int index;

        if (contextMenu == RootContextMenu)
        {
            index = 0;
        }
        else
        {
            index = _expandedItems.FindIndex(e => e.Item.ChildContextMenu == contextMenu);

            // Closed by this coordinator, which has already taken care of the path.
            if (index < 0)
                return;
        }

        await CollapseAsync(RemoveFrom(index), contextMenu);
    }

    /// <summary>
    /// Called when <paramref name="item"/> is removed from its context menu
    /// </summary>
    public async Task ItemRemovedAsync(IContextMenuItem item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return;

        await CollapseAsync(RemoveFrom(index), closingContextMenu: null);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var expandedItem in _expandedItems)
            expandedItem.Dispose();

        _expandedItems.Clear();
    }

    private async Task ShowChildContextMenuDelayedAsync(ExpandedContextMenuItem expandedItem)
    {
        var cancellationToken = expandedItem.CancellationToken;

        try
        {
            await Task.Delay(showChildContextMenuDelay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var childContextMenu = expandedItem.Item.ChildContextMenu;
        if (childContextMenu is null)
            return;

        var position = await expandedItem.Item.CalculateChildContextMenuPositionAsync();

        if (cancellationToken.IsCancellationRequested)
            return;

        // Without a position nothing is shown, so the item is collapsed and a later hover can try again.
        if (position is null)
        {
            await CollapseAsync(RemoveFrom(IndexOf(expandedItem.Item)), closingContextMenu: null);
            return;
        }

        expandedItem.DirectionTowardsChildContextMenu = position.MouseLeaveDirection;

        await childContextMenu.ShowAsync(new MouseEventArgs { PageX = position.X, PageY = position.Y },
            expandedItem.Item.ItemFilter);
    }

    private int IndexOf(IContextMenuItem item)
        => _expandedItems.FindIndex(e => e.Item == item);

    /// <summary>
    /// Removes the expanded items starting at <paramref name="index"/> and stops their pending child context menus
    /// </summary>
    private List<ExpandedContextMenuItem> RemoveFrom(int index)
    {
        if (index >= _expandedItems.Count)
            return [];

        var removedItems = _expandedItems[index..];
        _expandedItems.RemoveRange(index, removedItems.Count);

        foreach (var removedItem in removedItems)
            removedItem.Dispose();

        return removedItems;
    }

    /// <summary>
    /// Closes the child context menus of <paramref name="collapsedItems"/>, deepest first
    /// </summary>
    /// <param name="collapsedItems">Items already removed from the path by <see cref="RemoveFrom"/></param>
    /// <param name="closingContextMenu">Context menu that closes itself and must not be closed again</param>
    private static async Task CollapseAsync(List<ExpandedContextMenuItem> collapsedItems, IContextMenu? closingContextMenu)
    {
        for (var i = collapsedItems.Count - 1; i >= 0; i--)
        {
            var item = collapsedItems[i].Item;

            await item.EndObserveMouseLeaveAsync();

            if (item.ChildContextMenu is not null && item.ChildContextMenu != closingContextMenu)
                await item.ChildContextMenu.CloseAsync();
        }
    }
}
