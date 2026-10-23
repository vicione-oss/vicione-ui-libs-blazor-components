using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Accordion;

[Collection<ServerTestCollection>]
public class AccordionItemTests(ServerFixture fixture)
{
    private const string ScrollableContainerSelector = ".scroll-into-view-example";

    [Fact]
    public async Task Should_scroll_item_below_into_view()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var container = await GotoScrollableContainerAsync(page);

            var lastItemHeader = container.Locator(".accordion-item").Last.Locator(".header");
            await Expect(lastItemHeader).Not.ToBeInViewportAsync();

            // Act
            await page.GetByRole(AriaRole.Button, new() { Name = "Expand last" }).ClickAsync();

            // Assert
            await Expect(lastItemHeader).ToBeInViewportAsync(new() { Ratio = 1 });
        });
    }

    [Fact]
    public async Task Should_scroll_item_above_into_view()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var container = await GotoScrollableContainerAsync(page);

            var items = container.Locator(".accordion-item");
            var firstItemHeader = items.First.Locator(".header");

            await page.GetByRole(AriaRole.Button, new() { Name = "Expand last" }).ClickAsync();
            await Expect(items.Last.Locator(".header")).ToBeInViewportAsync(new() { Ratio = 1 });
            await Expect(firstItemHeader).Not.ToBeInViewportAsync();

            // Act
            await page.GetByRole(AriaRole.Button, new() { Name = "Expand first" }).ClickAsync();

            // Assert
            await Expect(firstItemHeader).ToBeInViewportAsync(new() { Ratio = 1 });
        });
    }

    private async Task<ILocator> GotoScrollableContainerAsync(IPage page)
    {
        await page.GotoAsync($"{fixture.ServerAddress}/accordion");

        // The page is prerendered, so a click before it turns interactive goes nowhere.
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var container = page.Locator(ScrollableContainerSelector);
        await container.ScrollIntoViewIfNeededAsync();

        return container;
    }
}
