using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.AdvancedTable;

// Browser-only behavior of the column filter chrome. None of this is reachable from bUnit: the drag
// suppression depends on how a real browser reads the draggable attribute and on native pointer events, and
// stopPropagation only actually stops anything in a real event pipeline.
[Collection<ServerTestCollection>]
public class ColumnFilterChromeTests(ServerFixture fixture)
{
    [Fact]
    public async Task Should_not_make_the_header_draggable_while_its_filter_panel_is_open()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/filter/custom-filters");

            var header = page.Locator("th[data-column-id='Key']").First;

            // A bool-valued attribute would render as draggable="", which the browser reads as "auto" —
            // so the literal strings are what the reorder behavior actually hangs off.
            await Expect(header).ToHaveAttributeAsync("draggable", "true");

            await header.Locator(".column-filter-button").ClickAsync();
            await Expect(page.Locator(".column-filter-panel").First).ToBeVisibleAsync();

            await Expect(header).ToHaveAttributeAsync("draggable", "false");
        });
    }

    [Fact]
    public async Task Should_not_start_a_column_reorder_when_the_filter_icon_is_pressed_and_dragged()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/filter/custom-filters");

            var headers = page.Locator("table.inner-table th[data-column-id]");
            var columnIdsBeforeDrag = await headers.EvaluateAllAsync<string[]>(
                "elements => elements.map(element => element.dataset.columnId)");

            var filterButton = page.Locator("th[data-column-id='Key'] .column-filter-button").First;
            var targetHeader = page.Locator("th[data-column-id='Value']").First;

            // Press on the icon and drag across the neighboring header — the pointer-down handler cancels
            // the native drag, so the column order must be untouched.
            await DragAsync(page, filterButton, targetHeader);

            var columnIdsAfterDrag = await headers.EvaluateAllAsync<string[]>(
                "elements => elements.map(element => element.dataset.columnId)");

            Assert.Equal(columnIdsBeforeDrag, columnIdsAfterDrag);
        });
    }

    [Fact]
    public async Task Should_not_sort_the_column_when_the_open_filter_panel_is_clicked()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/filter/custom-filters");

            var header = page.Locator("th[data-column-id='Key']").First;

            await header.Locator(".column-filter-button").ClickAsync();

            var panel = page.Locator(".column-filter-panel").First;
            await Expect(panel).ToBeVisibleAsync();

            var sortBeforeClick = await header.GetAttributeAsync("aria-sort");

            // The panel renders inside the header cell, so without stopPropagation a click on it would reach
            // the header's sort handler and the panel would fight the user.
            await panel.ClickAsync();

            await Expect(panel).ToBeVisibleAsync();
            await Expect(header).ToHaveAttributeAsync("aria-sort", sortBeforeClick ?? "none");
        });
    }

    private static async Task DragAsync(IPage page, ILocator source, ILocator target)
    {
        var sourceBox = await source.BoundingBoxAsync();
        var targetBox = await target.BoundingBoxAsync();

        Assert.NotNull(sourceBox);
        Assert.NotNull(targetBox);

        await page.Mouse.MoveAsync(sourceBox.X + (sourceBox.Width / 2), sourceBox.Y + (sourceBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(targetBox.X + (targetBox.Width / 2), targetBox.Y + (targetBox.Height / 2),
            new MouseMoveOptions { Steps = 10 });
        await page.Mouse.UpAsync();
    }
}
