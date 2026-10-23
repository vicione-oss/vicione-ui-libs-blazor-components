using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.ContextMenu.Components;

public sealed class ContextMenuCoordinatorTests
{
    private readonly IContextMenu _rootContextMenu = Substitute.For<IContextMenu>();

    [Fact]
    public async Task Should_show_child_context_menu_next_to_the_expanded_item()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var itemFilter = new ContextMenuItemFilter();
        var item = CreateItem(_rootContextMenu, itemFilter: itemFilter);

        // Act
        await coordinator.ExpandAsync(item);

        // Assert
        await item.ChildContextMenu!.Received(1)
            .ShowAsync(Arg.Is<MouseEventArgs>(e => e.PageX == 10 && e.PageY == 20), itemFilter);
        await item.Received(1).StartObserveMouseLeaveAsync();
    }

    [Fact]
    public async Task Should_not_show_child_context_menu_before_the_delay_has_elapsed()
    {
        // Arrange
        var coordinator = CreateCoordinator(Timeout.InfiniteTimeSpan);
        var item = CreateItem(_rootContextMenu);

        // Act
        var expandTask = coordinator.ExpandAsync(item);

        // Assert
        await item.ChildContextMenu!.DidNotReceiveWithAnyArgs().ShowAsync(default!, default);

        coordinator.Dispose();
        await expandTask;
    }

    [Fact]
    public async Task Should_show_child_context_menu_once_when_item_is_hovered_and_then_clicked()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);

        // Act
        await coordinator.ExpandAsync(item);
        await coordinator.ExpandAsync(item);

        // Assert
        await item.ChildContextMenu!.ReceivedWithAnyArgs(1).ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_close_child_context_menu_of_sibling_when_another_item_is_expanded()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var firstItem = CreateItem(_rootContextMenu);
        var secondItem = CreateItem(_rootContextMenu);

        await coordinator.ExpandAsync(firstItem);

        // Act
        await coordinator.ExpandAsync(secondItem);

        // Assert
        await firstItem.ChildContextMenu!.Received(1).CloseAsync();
        await firstItem.Received(1).EndObserveMouseLeaveAsync();
        await secondItem.ChildContextMenu!.ReceivedWithAnyArgs(1).ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_not_show_child_context_menu_of_sibling_whose_position_arrives_after_another_item_was_expanded()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var slowPosition = new TaskCompletionSource<ChildContextMenuPosition?>();
        var firstItem = CreateItem(_rootContextMenu);
        firstItem.CalculateChildContextMenuPositionAsync().Returns(slowPosition.Task);
        var secondItem = CreateItem(_rootContextMenu);

        var firstExpandTask = coordinator.ExpandAsync(firstItem);

        // Act
        await coordinator.ExpandAsync(secondItem);
        slowPosition.SetResult(new ChildContextMenuPosition(10, 20, MouseLeaveDirection.Right));
        await firstExpandTask;

        // Assert
        await firstItem.ChildContextMenu!.DidNotReceiveWithAnyArgs().ShowAsync(default!, default);
        await secondItem.ChildContextMenu!.ReceivedWithAnyArgs(1).ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_keep_child_context_menu_open_when_mouse_leaves_item_towards_it()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu, MouseLeaveDirection.Left);

        await coordinator.ExpandAsync(item);

        // Act
        await coordinator.MouseLeftItemAsync(item, MouseLeaveDirection.Left);

        // Assert
        await item.ChildContextMenu!.DidNotReceive().CloseAsync();
        await item.DidNotReceive().EndObserveMouseLeaveAsync();
    }

    [Fact]
    public async Task Should_close_child_context_menu_when_mouse_leaves_item_in_another_direction()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu, MouseLeaveDirection.Right);

        await coordinator.ExpandAsync(item);

        // Act
        await coordinator.MouseLeftItemAsync(item, MouseLeaveDirection.Bottom);

        // Assert
        await item.ChildContextMenu!.Received(1).CloseAsync();
        await item.Received(1).EndObserveMouseLeaveAsync();
    }

    [Fact]
    public async Task Should_not_show_pending_child_context_menu_when_mouse_leaves_item()
    {
        // Arrange
        using var coordinator = CreateCoordinator(Timeout.InfiniteTimeSpan);
        var item = CreateItem(_rootContextMenu, MouseLeaveDirection.Right);

        var expandTask = coordinator.ExpandAsync(item);

        // Act
        await coordinator.MouseLeftItemAsync(item, MouseLeaveDirection.Right);
        await expandTask;

        // Assert
        await item.ChildContextMenu!.DidNotReceiveWithAnyArgs().ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_close_all_child_context_menus_deepest_first_when_root_context_menu_closes()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);
        var nestedItem = CreateItem(item.ChildContextMenu!);

        // Read up front, as reading a substitute's property inside Received.InOrder() counts as a call.
        var childContextMenu = item.ChildContextMenu!;
        var nestedChildContextMenu = nestedItem.ChildContextMenu!;

        await coordinator.ExpandAsync(item);
        await coordinator.ExpandAsync(nestedItem);

        // Act
        await coordinator.ContextMenuClosingAsync(_rootContextMenu);

        // Assert
        Received.InOrder(() =>
        {
            nestedChildContextMenu.CloseAsync();
            childContextMenu.CloseAsync();
        });
    }

    [Fact]
    public async Task Should_close_all_child_context_menus_when_root_context_menu_is_shown_again()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);
        var nestedItem = CreateItem(item.ChildContextMenu!);

        await coordinator.ExpandAsync(item);
        await coordinator.ExpandAsync(nestedItem);

        // Act
        await coordinator.ContextMenuShowingAsync(_rootContextMenu);

        // Assert
        await item.ChildContextMenu!.Received(1).CloseAsync();
        await nestedItem.ChildContextMenu!.Received(1).CloseAsync();
    }

    [Fact]
    public async Task Should_close_only_deeper_child_context_menus_when_child_context_menu_is_shown_again()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);
        var nestedItem = CreateItem(item.ChildContextMenu!);

        await coordinator.ExpandAsync(item);
        await coordinator.ExpandAsync(nestedItem);

        // Act
        await coordinator.ContextMenuShowingAsync(item.ChildContextMenu!);

        // Assert
        await item.ChildContextMenu!.DidNotReceive().CloseAsync();
        await item.DidNotReceive().EndObserveMouseLeaveAsync();
        await nestedItem.ChildContextMenu!.Received(1).CloseAsync();
    }

    [Fact]
    public async Task Should_not_show_pending_child_context_menu_when_root_context_menu_is_shown_again()
    {
        // Arrange
        using var coordinator = CreateCoordinator(Timeout.InfiniteTimeSpan);
        var item = CreateItem(_rootContextMenu);

        var expandTask = coordinator.ExpandAsync(item);

        // Act
        await coordinator.ContextMenuShowingAsync(_rootContextMenu);
        await expandTask;

        // Assert
        await item.ChildContextMenu!.DidNotReceiveWithAnyArgs().ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_expand_item_again_after_its_child_context_menu_closed_itself()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);

        await coordinator.ExpandAsync(item);

        // Act
        await coordinator.ContextMenuClosingAsync(item.ChildContextMenu!);
        await coordinator.ExpandAsync(item);

        // Assert
        await item.ChildContextMenu!.DidNotReceive().CloseAsync();
        await item.ChildContextMenu!.ReceivedWithAnyArgs(2).ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_expand_item_again_after_its_child_context_menu_position_could_not_be_calculated()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);
        item.CalculateChildContextMenuPositionAsync().Returns(
            null,
            new ChildContextMenuPosition(10, 20, MouseLeaveDirection.Right));

        await coordinator.ExpandAsync(item);

        // Act
        await coordinator.ExpandAsync(item);

        // Assert
        await item.Received(2).CalculateChildContextMenuPositionAsync();
        await item.Received(1).EndObserveMouseLeaveAsync();
        await item.ChildContextMenu!.ReceivedWithAnyArgs(1).ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_ignore_item_whose_context_menu_is_not_open()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);
        var nestedItem = CreateItem(item.ChildContextMenu!);

        // Act
        await coordinator.ExpandAsync(nestedItem);

        // Assert
        await nestedItem.ChildContextMenu!.DidNotReceiveWithAnyArgs().ShowAsync(default!, default);
    }

    [Fact]
    public async Task Should_close_child_context_menu_when_its_item_is_removed()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        var item = CreateItem(_rootContextMenu);

        await coordinator.ExpandAsync(item);

        // Act
        await coordinator.ItemRemovedAsync(item);

        // Assert
        await item.ChildContextMenu!.Received(1).CloseAsync();
    }

    [Fact]
    public async Task Should_report_root_and_visible_child_context_menus_as_visible()
    {
        // Arrange
        using var coordinator = CreateCoordinator(TimeSpan.Zero);
        _rootContextMenu.Visible.Returns(true);
        var item = CreateItem(_rootContextMenu);
        item.ChildContextMenu!.Visible.Returns(true);
        var nestedItem = CreateItem(item.ChildContextMenu!);

        await coordinator.ExpandAsync(item);
        await coordinator.ExpandAsync(nestedItem);

        // Act
        var visibleContextMenus = coordinator.GetVisibleContextMenus();

        // Assert
        visibleContextMenus.Should().Equal(_rootContextMenu, item.ChildContextMenu);
    }

    private ContextMenuCoordinator CreateCoordinator(TimeSpan showChildContextMenuDelay)
        => new(_rootContextMenu, showChildContextMenuDelay);

    private static IContextMenuItem CreateItem(IContextMenu parentContextMenu,
        MouseLeaveDirection directionTowardsChildContextMenu = MouseLeaveDirection.Right,
        ContextMenuItemFilter? itemFilter = null)
    {
        var item = Substitute.For<IContextMenuItem>();
        var childContextMenu = Substitute.For<IContextMenu>();

        item.ParentContextMenu.Returns(parentContextMenu);
        item.ChildContextMenu.Returns(childContextMenu);
        item.ItemFilter.Returns(itemFilter);
        item.CalculateChildContextMenuPositionAsync()
            .Returns(new ChildContextMenuPosition(10, 20, directionTowardsChildContextMenu));

        return item;
    }
}
