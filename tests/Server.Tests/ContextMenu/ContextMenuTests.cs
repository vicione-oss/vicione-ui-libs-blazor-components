using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.ContextMenu;

// Child context menus open after the mouse pointer rests on an item, and close depending on where it moves next.
// That timing and those mouse movements only exist in a browser.
[Collection<ServerTestCollection>]
public class ContextMenuTests(ServerFixture fixture)
{
    // Longer than the delay before a child context menu is shown, so a child context menu that was still about to
    // show would have shown by then.
    private const int ShowChildContextMenuSettleDelay = 500;

    private const int OpenAttempts = 10;

    private static readonly TimeSpan s_openAttemptTimeout = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task Should_close_child_context_menu_of_sibling_when_another_item_is_hovered()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenFirstContextMenuAsync(page);

            await MenuItem(page, "Editing").HoverAsync();
            await Expect(ContextMenuWithItem(page, "Remove")).ToBeVisibleAsync();

            // Act
            await MenuItem(page, "Selection").HoverAsync();

            // Assert
            await Expect(ContextMenuWithItem(page, "Select nodes")).ToBeVisibleAsync();
            await Expect(ContextMenuWithItem(page, "Remove")).Not.ToBeVisibleAsync();
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(2);
        });
    }

    [Fact]
    public async Task Should_not_show_child_context_menu_of_item_hovered_only_briefly()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenFirstContextMenuAsync(page);

            // Act
            await MenuItem(page, "Editing").HoverAsync();
            await MenuItem(page, "Selection").HoverAsync();

            // Assert
            await Expect(ContextMenuWithItem(page, "Select nodes")).ToBeVisibleAsync();

            await page.WaitForTimeoutAsync(ShowChildContextMenuSettleDelay);

            await Expect(ContextMenuWithItem(page, "Remove")).Not.ToBeVisibleAsync();
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(2);
        });
    }

    [Fact]
    public async Task Should_keep_child_context_menu_open_while_mouse_moves_onto_it()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenFirstContextMenuAsync(page);

            var selectionItem = MenuItem(page, "Selection");
            var selectionContextMenu = ContextMenuWithItem(page, "Select nodes");

            await selectionItem.HoverAsync();
            await Expect(selectionContextMenu).ToBeVisibleAsync();

            var itemBox = await selectionItem.BoundingBoxAsync();
            var childContextMenuBox = await selectionContextMenu.BoundingBoxAsync();

            Assert.NotNull(itemBox);
            Assert.NotNull(childContextMenuBox);

            // Act
            // Leaves the item sideways, straight into the child context menu.
            await page.Mouse.MoveAsync(childContextMenuBox.X + (childContextMenuBox.Width / 2),
                itemBox.Y + (itemBox.Height / 2), new() { Steps = 5 });

            await MenuItem(page, "Select faces").HoverAsync();

            // Assert
            await Expect(ContextMenuWithItem(page, "Left face")).ToBeVisibleAsync();
            await Expect(selectionContextMenu).ToBeVisibleAsync();
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(3);
        });
    }

    [Fact]
    public async Task Should_close_all_context_menus_when_item_of_child_context_menu_is_clicked()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenSelectFacesContextMenuAsync(page);

            // Act
            await MenuItem(page, "Left face").ClickAsync();

            // Assert
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(0);
            await Expect(page.Locator("li", new() { HasText = "ContextMenuItemClicked" })).ToContainTextAsync("True");
        });
    }

    [Fact]
    public async Task Should_close_all_context_menus_when_clicked_outside()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenSelectFacesContextMenuAsync(page);

            // Act
            await page.GetByText("First Context Menu Visible").ClickAsync();

            // Assert
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(0);
            await Expect(page.Locator("li", new() { HasText = "First Context Menu Visible" })).ToContainTextAsync("False");
        });
    }

    [Fact]
    public async Task Should_show_child_context_menu_again_after_all_context_menus_were_closed()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await OpenFirstContextMenuAsync(page);

            await MenuItem(page, "Selection").HoverAsync();
            await Expect(ContextMenuWithItem(page, "Select nodes")).ToBeVisibleAsync();

            await page.GetByText("First Context Menu Visible").ClickAsync();
            await Expect(VisibleContextMenus(page)).ToHaveCountAsync(0);

            // Act
            await OpenFirstContextMenuAsync(page, navigate: false);
            await MenuItem(page, "Selection").HoverAsync();

            // Assert
            await Expect(ContextMenuWithItem(page, "Select nodes")).ToBeVisibleAsync();
        });
    }

    private async Task OpenSelectFacesContextMenuAsync(IPage page)
    {
        await OpenFirstContextMenuAsync(page);

        await MenuItem(page, "Selection").HoverAsync();
        await Expect(ContextMenuWithItem(page, "Select nodes")).ToBeVisibleAsync();

        await MenuItem(page, "Select faces").HoverAsync();
        await Expect(ContextMenuWithItem(page, "Left face")).ToBeVisibleAsync();
    }

    private async Task OpenFirstContextMenuAsync(IPage page, bool navigate = true)
    {
        if (navigate)
            await page.GotoAsync($"{fixture.ServerAddress}/context-menu");

        var requestArea = page.Locator(".context-menu-request-area").First;
        var rootContextMenu = page.Locator(".context-menu", new() { HasText = "Example context menu" });

        // The page is prerendered, so a right click that arrives before the circuit is connected opens nothing.
        for (var attempt = 1; ; attempt++)
        {
            await requestArea.ClickAsync(new() { Button = MouseButton.Right, Position = new() { X = 10, Y = 10 } });

            try
            {
                await Expect(rootContextMenu).ToBeVisibleAsync(new() { Timeout = (float)s_openAttemptTimeout.TotalMilliseconds });
                return;
            }
            catch (PlaywrightException) when (attempt < OpenAttempts)
            {
                // Not interactive yet, try again.
            }
        }
    }

    private static ILocator MenuItem(IPage page, string text)
        => page.GetByRole(AriaRole.Menuitem, new() { Name = text, Exact = true });

    private static ILocator ContextMenuWithItem(IPage page, string itemText)
        => page.Locator(".context-menu", new() { Has = MenuItem(page, itemText) });

    private static ILocator VisibleContextMenus(IPage page)
        => page.Locator(".context-menu:visible");
}
