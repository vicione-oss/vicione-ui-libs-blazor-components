using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.TabStrip;

[Collection<ServerTestCollection>]
public class TabStripTests(ServerFixture fixture)
{
    private const double EdgeTolerance = 8;

    private static readonly Regex s_activeScrollButtonClass = new(@"(^|\s)active(\s|$)");

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_disable_right_scroll_button_after_scrolling_to_the_end(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (_, scrollContainer, leftButton, rightButton) = await GetScrollingTabStripAsync(page);

            // Initially the strip is scrolled to the start: left disabled, right enabled.
            await Expect(leftButton).Not.ToHaveClassAsync(s_activeScrollButtonClass);
            await Expect(rightButton).ToHaveClassAsync(s_activeScrollButtonClass);

            await ClickScrollButtonUntilDisabledAsync(rightButton, scrollContainer);

            // After scrolling to the end: right disabled, left enabled.
            await Expect(rightButton).Not.ToHaveClassAsync(s_activeScrollButtonClass);
            await Expect(leftButton).ToHaveClassAsync(s_activeScrollButtonClass);
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_disable_left_scroll_button_after_scrolling_to_the_start(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (_, scrollContainer, leftButton, rightButton) = await GetScrollingTabStripAsync(page);

            // Arrange: scroll to the end so the left scroll button becomes enabled.
            await ClickScrollButtonUntilDisabledAsync(rightButton, scrollContainer);
            await Expect(leftButton).ToHaveClassAsync(s_activeScrollButtonClass);

            // Act: scroll back to the start.
            await ClickScrollButtonUntilDisabledAsync(leftButton, scrollContainer);

            // After scrolling to the start: left disabled, right enabled.
            await Expect(leftButton).Not.ToHaveClassAsync(s_activeScrollButtonClass);
            await Expect(rightButton).ToHaveClassAsync(s_activeScrollButtonClass);
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_right_overflow_when_clicked(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (tabStrip, scrollContainer, _, _) = await GetScrollingTabStripAsync(page);
            await WaitForScrollToSettleAsync(scrollContainer);

            var tabs = tabStrip.Locator(".tabs > .tab");
            var overflowSize = await GetOverflowSizeAsync(scrollContainer);

            var containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            // While scrolled to the start, find the first tab hidden behind the right overflow gradient.
            var clippedTabIndex = await FindFirstTabClippedOnRightAsync(tabs, containerBox.X + containerBox.Width - overflowSize);
            Assert.True(clippedTabIndex >= 0, "Expected a tab hidden behind the right overflow gradient.");

            // Dispatch the click so Playwright does not scroll the tab into view first; the component must scroll it.
            var clippedTab = tabs.Nth(clippedTabIndex);
            var scrollLeftBeforeSelection = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");
            await clippedTab.DispatchEventAsync("click");
            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeSelection);

            // The clicked tab is now fully visible, flush with the first visible position after the left overflow area.
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleLeft = containerBox.X + overflowSize;
            Assert.True(Math.Abs(tabBox.X - visibleLeft) <= EdgeTolerance,
                $"Expected tab left edge at {visibleLeft}px (+/-{EdgeTolerance}px), but was {tabBox.X}px.");
            Assert.True(tabBox.X + tabBox.Width <= containerBox.X + containerBox.Width + 1,
                "Expected the clicked tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_left_overflow_when_clicked(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (tabStrip, scrollContainer, _, rightButton) = await GetScrollingTabStripAsync(page);

            // Arrange: scroll to the end so leading tabs become hidden behind the left overflow gradient. Scrolling all
            // the way ensures the clipped tab sits mid-strip, so revealing it does not clamp against the scroll start.
            await ClickScrollButtonUntilDisabledAsync(rightButton, scrollContainer);

            var tabs = tabStrip.Locator(".tabs > .tab");
            var overflowSize = await GetOverflowSizeAsync(scrollContainer);

            var containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            // Find the last tab hidden behind the left overflow gradient.
            var clippedTabIndex = await FindLastTabClippedOnLeftAsync(tabs, containerBox.X + overflowSize);
            Assert.True(clippedTabIndex >= 0, "Expected a tab hidden behind the left overflow gradient.");

            // Dispatch the click so Playwright does not scroll the tab into view first; the component must scroll it.
            var clippedTab = tabs.Nth(clippedTabIndex);
            var scrollLeftBeforeSelection = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");
            await clippedTab.DispatchEventAsync("click");
            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeSelection);

            // The clicked tab is now fully visible, flush with the last visible position before the right overflow area.
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleRight = containerBox.X + containerBox.Width - overflowSize;
            Assert.True(Math.Abs(tabBox.X + tabBox.Width - visibleRight) <= EdgeTolerance,
                $"Expected tab right edge at {visibleRight}px (+/-{EdgeTolerance}px), but was {tabBox.X + tabBox.Width}px.");
            Assert.True(tabBox.X >= containerBox.X - 1,
                "Expected the clicked tab to be fully visible within the scroll container.");
        });
    }

    private static async Task<(ILocator TabStrip, ILocator ScrollContainer, ILocator LeftButton, ILocator RightButton)>
        GetScrollingTabStripAsync(IPage page)
    {
        var tabStrip = page.Locator(".tabs-with-scrolling .tab-strip");
        var scrollContainer = tabStrip.Locator(".tabs-scroll-container");
        var scrollButtons = tabStrip.Locator(".tab-strip-scroll-button");

        // Wait until the JavaScript has attached and the strip is no longer hidden by the "invisible" class.
        await Expect(scrollButtons.Last).ToBeVisibleAsync();

        return (tabStrip, scrollContainer, scrollButtons.First, scrollButtons.Last);
    }

    private static async Task SelectTabSizeAsync(IPage page, string tabSize)
    {
        var sizeSelect = page.Locator(".combo-box select").First;
        await sizeSelect.SelectOptionAsync(new SelectOptionValue { Label = tabSize });
    }

    private static async Task ClickScrollButtonUntilDisabledAsync(ILocator scrollButton, ILocator scrollContainer)
    {
        for (var click = 0; click < 30 && await IsScrollButtonActiveAsync(scrollButton); click++)
        {
            await scrollButton.ClickAsync();
            await WaitForScrollToSettleAsync(scrollContainer);
        }
    }

    private static async Task<bool> IsScrollButtonActiveAsync(ILocator scrollButton)
    {
        var classAttribute = await scrollButton.GetAttributeAsync("class");
        return classAttribute?.Contains("active", StringComparison.Ordinal) == true;
    }

    private static async Task<int> FindFirstTabClippedOnRightAsync(ILocator tabs, double visibleRight)
    {
        var count = await tabs.CountAsync();

        for (var index = 0; index < count; index++)
        {
            var tabBox = await tabs.Nth(index).BoundingBoxAsync();

            if (tabBox is null)
                continue;

            if (tabBox.X + tabBox.Width > visibleRight + 1)
                return index;
        }

        return -1;
    }

    private static async Task<int> FindLastTabClippedOnLeftAsync(ILocator tabs, double visibleLeft)
    {
        var count = await tabs.CountAsync();

        for (var index = count - 1; index >= 0; index--)
        {
            var tabBox = await tabs.Nth(index).BoundingBoxAsync();

            if (tabBox is null)
                continue;

            if (tabBox.X < visibleLeft - 1)
                return index;
        }

        return -1;
    }

    private static async Task WaitForSelectionScrollAsync(ILocator scrollContainer, double scrollLeftBeforeSelection)
    {
        // Wait until the component starts scrolling the selected tab into view, then wait for that scroll to settle.
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var scrollLeft = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            if (Math.Abs(scrollLeft - scrollLeftBeforeSelection) >= 1)
                break;

            await Task.Delay(50);
        }

        await WaitForScrollToSettleAsync(scrollContainer);
    }

    private static async Task WaitForScrollToSettleAsync(ILocator scrollContainer)
    {
        var previousScrollLeft = double.NaN;
        var stableReads = 0;

        for (var attempt = 0; attempt < 60; attempt++)
        {
            var scrollLeft = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            if (!double.IsNaN(previousScrollLeft) && Math.Abs(scrollLeft - previousScrollLeft) < 0.5)
            {
                if (++stableReads >= 3)
                    return;
            }
            else
            {
                stableReads = 0;
            }

            previousScrollLeft = scrollLeft;
            await Task.Delay(80);
        }
    }

    private static async Task<double> GetOverflowSizeAsync(ILocator scrollContainer)
    {
        var rawValue = await scrollContainer.EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--tab-strip-overflow-size')");

        var numericValue = rawValue.Replace("px", string.Empty, StringComparison.Ordinal).Trim();

        return double.TryParse(numericValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var overflowSize)
            ? overflowSize
            : 0;
    }
}
