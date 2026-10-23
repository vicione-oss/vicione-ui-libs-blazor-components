using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.Shared;

// Column reorder runs on native drag and drop and column resize on pointer events from the same header, so only a
// real browser shows whether one gesture starts the other.
[Collection<ServerTestCollection>]
public class ColumnReorderTests(ServerFixture fixture)
{
    // The SimpleTable page mirrors this one, so the same selectors address both.
    private const string AdvancedTablePagePath = "/tables/advanced-table/column-reorder";

    private const string SimpleTablePagePath = "/tables/simple-table/column-reorder";

    private const int ResizeDistance = 80;

    [Theory]
    [InlineData(AdvancedTablePagePath)]
    [InlineData(SimpleTablePagePath)]
    public async Task Should_move_a_column_dropped_on_a_gap(string pagePath)
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}{pagePath}");

            // Act
            await DragHeaderOntoTheGapBeforeKeyAsync(page, "Timestamp");

            // Assert
            await Expect(Headers(page).First).ToHaveAttributeAsync("data-column-id", "Timestamp");
        });
    }

    [Theory]
    [InlineData(AdvancedTablePagePath)]
    [InlineData(SimpleTablePagePath)]
    public async Task Should_keep_the_column_order_when_column_reordering_is_turned_off(string pagePath)
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoWithColumnReorderTurnedOffAsync(page, pagePath);

            var timestampHeader = page.Locator("th[data-column-id='Timestamp']");
            var keyHeader = page.Locator("th[data-column-id='Key']");

            // Act
            await PressAsync(page, timestampHeader);
            await MoveToAsync(page, keyHeader);

            // Assert
            await Expect(page.Locator(".gap-zone")).ToHaveCountAsync(0);

            await page.Mouse.UpAsync();

            Assert.Equal(["Key", "Value", "Timestamp"], await ColumnOrderAsync(page));
        });
    }

    [Theory]
    [InlineData(AdvancedTablePagePath, true)]
    [InlineData(AdvancedTablePagePath, false)]
    [InlineData(SimpleTablePagePath, true)]
    [InlineData(SimpleTablePagePath, false)]
    public async Task Should_resize_a_column_without_starting_a_reorder(string pagePath, bool columnReorderEnabled)
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            if (columnReorderEnabled)
                await page.GotoAsync($"{fixture.ServerAddress}{pagePath}");
            else
                await GotoWithColumnReorderTurnedOffAsync(page, pagePath);

            var keyHeader = page.Locator("th[data-column-id='Key']");
            var widthBefore = await WidthAsync(keyHeader);

            // Act
            var handleBox = await keyHeader.Locator(".resize-handle").BoundingBoxAsync();
            Assert.NotNull(handleBox);

            var handleX = handleBox.X + (handleBox.Width / 2);
            var handleY = handleBox.Y + (handleBox.Height / 2);

            await page.Mouse.MoveAsync(handleX, handleY);
            await page.Mouse.DownAsync();
            await page.Mouse.MoveAsync(handleX + ResizeDistance, handleY, new MouseMoveOptions { Steps = 10 });

            // Assert
            await Expect(page.Locator(".gap-zone")).ToHaveCountAsync(0);

            await page.Mouse.UpAsync();

            var widthAfter = await WidthAsync(keyHeader);

            Assert.True(widthAfter - widthBefore > ResizeDistance / 2,
                $"the column did not follow the handle: {widthBefore} -> {widthAfter}");

            Assert.Equal(["Key", "Value", "Timestamp"], await ColumnOrderAsync(page));
        });
    }

    private async Task GotoWithColumnReorderTurnedOffAsync(IPage page, string pagePath)
    {
        await page.GotoAsync($"{fixture.ServerAddress}{pagePath}");
        await page.Locator(".options .switch").ClickAsync();

        await Expect(Headers(page).First).ToHaveAttributeAsync("draggable", "false");
    }

    // The gap zones only exist once a drag is under way, so the drop target is looked up mid-drag.
    private static async Task DragHeaderOntoTheGapBeforeKeyAsync(IPage page, string columnId)
    {
        await PressAsync(page, page.Locator($"th[data-column-id='{columnId}']"));
        await MoveToAsync(page, page.Locator("th[data-column-id='Key']"));
        await MoveToAsync(page, page.Locator("th[data-column-id='Key'] .gap-zone-start"));
        await page.Mouse.UpAsync();
    }

    private static async Task PressAsync(IPage page, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2));
        await page.Mouse.DownAsync();
    }

    private static async Task MoveToAsync(IPage page, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.MoveAsync(box.X + (box.Width / 2), box.Y + (box.Height / 2),
            new MouseMoveOptions { Steps = 10 });
    }

    private static ILocator Headers(IPage page)
        => page.Locator("table.inner-table th[data-column-id]");

    private static Task<string[]> ColumnOrderAsync(IPage page)
        => Headers(page).EvaluateAllAsync<string[]>("elements => elements.map(element => element.dataset.columnId)");

    private static async Task<float> WidthAsync(ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        Assert.NotNull(box);

        return box.Width;
    }
}
