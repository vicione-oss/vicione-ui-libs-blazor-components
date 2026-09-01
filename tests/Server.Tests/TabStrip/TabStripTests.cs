using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.TabStrip;

[Collection<ServerTestCollection>]
public partial class TabStripTests(ServerFixture fixture)
{
    private const double EdgeTolerance = 8;

    private const string BeforeFadeColorProperty = "--tabs-viewport-before-fade-color";
    private const string AfterFadeColorProperty = "--tabs-viewport-after-fade-color";

    [GeneratedRegex(@"(^|\s)active(\s|$)")]
    private static partial Regex ActiveScrollButtonClass { get; }

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
            await Expect(leftButton).Not.ToHaveClassAsync(ActiveScrollButtonClass);
            await Expect(rightButton).ToHaveClassAsync(ActiveScrollButtonClass);

            await ClickScrollButtonUntilDisabledAsync(rightButton, scrollContainer);

            // After scrolling to the end: right disabled, left enabled.
            await Expect(rightButton).Not.ToHaveClassAsync(ActiveScrollButtonClass);
            await Expect(leftButton).ToHaveClassAsync(ActiveScrollButtonClass);
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
            await Expect(leftButton).ToHaveClassAsync(ActiveScrollButtonClass);

            // Act: scroll back to the start.
            await ClickScrollButtonUntilDisabledAsync(leftButton, scrollContainer);

            // After scrolling to the start: left disabled, right enabled.
            await Expect(leftButton).Not.ToHaveClassAsync(ActiveScrollButtonClass);
            await Expect(rightButton).ToHaveClassAsync(ActiveScrollButtonClass);
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

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_right_overflow_when_navigating_with_arrow_key(string tabSize)
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
            Assert.True(clippedTabIndex >= 1, "Expected a tab hidden behind the right overflow gradient to navigate to.");

            // Focus the first tab, then walk focus one item at a time towards the clipped tab using the right arrow key.
            await tabs.First.FocusAsync();
            Assert.Equal(0, await GetFocusedTabIndexAsync(tabs));

            var scrollLeftBeforeNavigation = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            for (var expectedIndex = 1; expectedIndex <= clippedTabIndex; expectedIndex++)
            {
                await page.Keyboard.PressAsync("ArrowRight");

                // Each arrow key press must advance focus by exactly one item instead of scrolling by a few pixels.
                Assert.Equal(expectedIndex, await GetFocusedTabIndexAsync(tabs));
            }

            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeNavigation);

            // The focused tab is now fully visible, flush with the first visible position after the left overflow area.
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var clippedTab = tabs.Nth(clippedTabIndex);
            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleLeft = containerBox.X + overflowSize;
            Assert.True(Math.Abs(tabBox.X - visibleLeft) <= EdgeTolerance,
                $"Expected tab left edge at {visibleLeft}px (+/-{EdgeTolerance}px), but was {tabBox.X}px.");
            Assert.True(tabBox.X + tabBox.Width <= containerBox.X + containerBox.Width + 1,
                "Expected the focused tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_left_overflow_when_navigating_with_arrow_key(string tabSize)
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
            var tabCount = await tabs.CountAsync();
            var overflowSize = await GetOverflowSizeAsync(scrollContainer);

            var containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            // Find the last tab hidden behind the left overflow gradient.
            var clippedTabIndex = await FindLastTabClippedOnLeftAsync(tabs, containerBox.X + overflowSize);
            Assert.True(clippedTabIndex >= 0, "Expected a tab hidden behind the left overflow gradient.");
            Assert.True(clippedTabIndex < tabCount - 1, "Expected the clipped tab to be navigable from the last tab.");

            // Focus the last tab, then walk focus one item at a time towards the clipped tab using the left arrow key.
            await tabs.Last.FocusAsync();
            Assert.Equal(tabCount - 1, await GetFocusedTabIndexAsync(tabs));

            var scrollLeftBeforeNavigation = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            for (var expectedIndex = tabCount - 2; expectedIndex >= clippedTabIndex; expectedIndex--)
            {
                await page.Keyboard.PressAsync("ArrowLeft");

                // Each arrow key press must move focus back by exactly one item instead of scrolling by a few pixels.
                Assert.Equal(expectedIndex, await GetFocusedTabIndexAsync(tabs));
            }

            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeNavigation);

            // The focused tab is now fully visible, flush with the last visible position before the right overflow area.
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var clippedTab = tabs.Nth(clippedTabIndex);
            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleRight = containerBox.X + containerBox.Width - overflowSize;
            Assert.True(Math.Abs(tabBox.X + tabBox.Width - visibleRight) <= EdgeTolerance,
                $"Expected tab right edge at {visibleRight}px (+/-{EdgeTolerance}px), but was {tabBox.X + tabBox.Width}px.");
            Assert.True(tabBox.X >= containerBox.X - 1,
                "Expected the focused tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_right_overflow_when_focused_with_tab_key(string tabSize)
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
            Assert.True(clippedTabIndex >= 1, "Expected a tab hidden behind the right overflow gradient to tab to.");

            // Focus the first tab, then move focus forward one item at a time using the Tab key. Unlike the arrow keys,
            // the Tab key moves native browser focus, so the reveal is driven by the component's focusin handler.
            await tabs.First.FocusAsync();
            Assert.Equal(0, await GetFocusedTabIndexAsync(tabs));

            var scrollLeftBeforeNavigation = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            for (var expectedIndex = 1; expectedIndex <= clippedTabIndex; expectedIndex++)
            {
                await page.Keyboard.PressAsync("Tab");

                // Focusing the next tab must advance focus by exactly one item so it can be revealed.
                Assert.Equal(expectedIndex, await GetFocusedTabIndexAsync(tabs));
            }

            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeNavigation);

            // The focused tab is now fully visible, flush with the first visible position after the left overflow area
            // (native focus scrolling would leave it under the gradient; only the component aligns past the overflow).
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var clippedTab = tabs.Nth(clippedTabIndex);
            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleLeft = containerBox.X + overflowSize;
            Assert.True(Math.Abs(tabBox.X - visibleLeft) <= EdgeTolerance,
                $"Expected tab left edge at {visibleLeft}px (+/-{EdgeTolerance}px), but was {tabBox.X}px.");
            Assert.True(tabBox.X + tabBox.Width <= containerBox.X + containerBox.Width + 1,
                "Expected the focused tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_reveal_tab_hidden_behind_left_overflow_when_focused_with_shift_tab_key(string tabSize)
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
            var tabCount = await tabs.CountAsync();
            var overflowSize = await GetOverflowSizeAsync(scrollContainer);

            var containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            // Find the last tab hidden behind the left overflow gradient.
            var clippedTabIndex = await FindLastTabClippedOnLeftAsync(tabs, containerBox.X + overflowSize);
            Assert.True(clippedTabIndex >= 0, "Expected a tab hidden behind the left overflow gradient.");
            Assert.True(clippedTabIndex < tabCount - 1, "Expected the clipped tab to be reachable from the last tab.");

            // Focus the last tab, then move focus backward one item at a time using Shift+Tab. As with the Tab key, the
            // reveal is driven by the component's focusin handler rather than the arrow-key handler.
            await tabs.Last.FocusAsync();
            Assert.Equal(tabCount - 1, await GetFocusedTabIndexAsync(tabs));

            var scrollLeftBeforeNavigation = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            for (var expectedIndex = tabCount - 2; expectedIndex >= clippedTabIndex; expectedIndex--)
            {
                await page.Keyboard.PressAsync("Shift+Tab");

                // Focusing the previous tab must move focus back by exactly one item so it can be revealed.
                Assert.Equal(expectedIndex, await GetFocusedTabIndexAsync(tabs));
            }

            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeNavigation);

            // The focused tab is now fully visible, flush with the last visible position before the right overflow area.
            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var clippedTab = tabs.Nth(clippedTabIndex);
            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleRight = containerBox.X + containerBox.Width - overflowSize;
            Assert.True(Math.Abs(tabBox.X + tabBox.Width - visibleRight) <= EdgeTolerance,
                $"Expected tab right edge at {visibleRight}px (+/-{EdgeTolerance}px), but was {tabBox.X + tabBox.Width}px.");
            Assert.True(tabBox.X >= containerBox.X - 1,
                "Expected the focused tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_select_and_reveal_partially_visible_tab_when_clicked(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (tabStrip, scrollContainer, _, _) = await GetScrollingTabStripAsync(page);
            await WaitForScrollToSettleAsync(scrollContainer);

            var tabs = tabStrip.Locator(".tabs > .tab");
            var tabCount = await tabs.CountAsync();
            var overflowSize = await GetOverflowSizeAsync(scrollContainer);

            // Pick a middle tab and scroll so exactly its left half is visible while its right half is clipped by the
            // container's right edge. Driving the scroll position ourselves (instead of relying on where tab/margin
            // boundaries happen to fall) makes the partially-visible geometry deterministic across tab sizes.
            var clippedTabIndex = tabCount / 2;
            var clippedTab = tabs.Nth(clippedTabIndex);

            var offsetLeft = await clippedTab.EvaluateAsync<double>("element => element.offsetLeft");
            var offsetWidth = await clippedTab.EvaluateAsync<double>("element => element.offsetWidth");
            var clientWidth = await scrollContainer.EvaluateAsync<double>("element => element.clientWidth");

            var targetScrollLeft = Math.Max(0, offsetLeft - clientWidth + (offsetWidth / 2));
            await scrollContainer.EvaluateAsync("(element, scrollLeft) => element.scrollTo({ left: scrollLeft })", targetScrollLeft);
            await WaitForScrollToSettleAsync(scrollContainer);

            var containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            var containerRight = containerBox.X + containerBox.Width;
            var tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            // Confirm the tab really straddles the right edge: part visible (clickable), part clipped. The gradient
            // overlay is pointer-events:none, so a click in that region still lands on the tab beneath it.
            Assert.True(tabBox.X < containerRight, "Expected part of the clipped tab to be visible so it can be clicked.");
            Assert.True(tabBox.X + tabBox.Width > containerRight, "Expected part of the clipped tab to be clipped by the right edge.");

            // Click the still-visible left portion using raw mouse coordinates. A normal locator click would let
            // Playwright scroll the tab fully into view first, hiding the very race this test guards against: the
            // pointerdown focuses the tab, and if the focusin handler scrolled it away the click would be cancelled.
            var clickX = (float)((tabBox.X + Math.Min(tabBox.X + tabBox.Width, containerRight)) / 2);
            var clickY = tabBox.Y + (tabBox.Height / 2);

            var scrollLeftBeforeClick = await scrollContainer.EvaluateAsync<double>("element => element.scrollLeft");

            await page.Mouse.MoveAsync(clickX, clickY);
            await page.Mouse.ClickAsync(clickX, clickY);

            // The click must select the tab (not merely scroll it).
            await Expect(clippedTab).ToHaveClassAsync(ActiveScrollButtonClass);

            // Selection then reveals the tab: it ends up fully visible, flush before the right overflow area.
            await WaitForSelectionScrollAsync(scrollContainer, scrollLeftBeforeClick);

            containerBox = await scrollContainer.BoundingBoxAsync();
            Assert.NotNull(containerBox);

            tabBox = await clippedTab.BoundingBoxAsync();
            Assert.NotNull(tabBox);

            var visibleRight = containerBox.X + containerBox.Width - overflowSize;
            Assert.True(tabBox.X + tabBox.Width <= visibleRight + EdgeTolerance,
                $"Expected the selected tab right edge to be at most {visibleRight}px (+/-{EdgeTolerance}px), but was {tabBox.X + tabBox.Width}px.");
            Assert.True(tabBox.X >= containerBox.X - 1,
                "Expected the selected tab to be fully visible within the scroll container.");
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_apply_overflow_fade_colors_when_tab_strip_is_visible(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (tabStrip, _, _, _) = await GetScrollingTabStripAsync(page);
            var tabsViewport = tabStrip.Locator(".tabs-viewport");

            // While the strip is on-screen the IntersectionObserver resolves the left (::before) and right (::after)
            // gradient fade colors and writes them as inline custom properties on the tabs viewport.
            var beforeFadeColor = await WaitForFadeColorAsync(tabsViewport, BeforeFadeColorProperty);
            var afterFadeColor = await WaitForFadeColorAsync(tabsViewport, AfterFadeColorProperty);

            Assert.StartsWith("rgb", beforeFadeColor, StringComparison.Ordinal);
            Assert.StartsWith("rgb", afterFadeColor, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData("Large")]
    [InlineData("Small")]
    public async Task Should_only_recompute_overflow_fade_colors_while_tab_strip_is_visible(string tabSize)
    {
        var browser = new Browser().WithOptions(new() { SlowMo = 200 });

        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tab-strip");
            await SelectTabSizeAsync(page, tabSize);

            var (tabStrip, _, _, _) = await GetScrollingTabStripAsync(page);
            var tabsViewport = tabStrip.Locator(".tabs-viewport");

            // The fade colors are resolved once the strip becomes visible on load.
            Assert.StartsWith("rgb", await WaitForFadeColorAsync(tabsViewport, BeforeFadeColorProperty), StringComparison.Ordinal);

            // Move the strip completely out of the viewport, then clear the resolved colors so a later recompute
            // (or the absence of one) becomes observable.
            await ScrollTabStripOutOfViewAsync(page, tabsViewport);
            Assert.False(await IsTabStripInViewportAsync(tabsViewport), "Expected the tab strip to be scrolled out of view.");
            await ClearFadeColorsAsync(tabsViewport);

            // Resizing normally recomputes the fade colors (ResizeObserver), but while the strip is off-screen the
            // computation must be skipped so probe points are never resolved outside the viewport. Only the width is
            // changed so the vertical scroll position - and thus the off-screen state - is preserved.
            var viewport = page.ViewportSize;
            Assert.NotNull(viewport);
            await page.SetViewportSizeAsync(viewport.Width - 100, viewport.Height);
            await Task.Delay(300);

            Assert.False(await IsTabStripInViewportAsync(tabsViewport), "Expected the tab strip to remain out of view after resizing.");
            Assert.Equal(string.Empty, await GetInlineFadeColorAsync(tabsViewport, BeforeFadeColorProperty));
            Assert.Equal(string.Empty, await GetInlineFadeColorAsync(tabsViewport, AfterFadeColorProperty));

            // Scrolling the strip back into view must recompute the fade colors again.
            await ScrollTabStripIntoViewAsync(tabsViewport);

            Assert.StartsWith("rgb", await WaitForFadeColorAsync(tabsViewport, BeforeFadeColorProperty), StringComparison.Ordinal);
            Assert.StartsWith("rgb", await WaitForFadeColorAsync(tabsViewport, AfterFadeColorProperty), StringComparison.Ordinal);
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
        var comboBox = page.Locator(".combo-box").First;

        // The reworked ComboBox renders a text input plus a custom dropdown (no native <select>), so the tab size is
        // chosen by opening the dropdown via the input and clicking the option whose text matches the requested size.
        await comboBox.Locator("input.combo-box-input").ClickAsync();

        var option = comboBox.Locator(".drop-down-item[role='option']").Filter(new() { HasTextString = tabSize });
        await option.First.ClickAsync();
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

    private static async Task<int> GetFocusedTabIndexAsync(ILocator tabs)
    {
        var count = await tabs.CountAsync();

        for (var index = 0; index < count; index++)
        {
            var isFocused = await tabs.Nth(index).EvaluateAsync<bool>("element => element === document.activeElement");

            if (isFocused)
                return index;
        }

        return -1;
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

        if (double.TryParse(numericValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var overflowSize))
            return overflowSize;

        return 0;
    }

    private static Task<string> GetInlineFadeColorAsync(ILocator viewport, string property) => viewport.EvaluateAsync<string>(
        "(element, name) => element.style.getPropertyValue(name)", property);

    private static async Task<string> WaitForFadeColorAsync(ILocator tabsViewport, string propertyName)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var value = await GetInlineFadeColorAsync(tabsViewport, propertyName);

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            await Task.Delay(50);
        }

        return string.Empty;
    }

    private static async Task ClearFadeColorsAsync(ILocator viewport) => await viewport.EvaluateAsync(
        @"element => {
                element.style.removeProperty('--tabs-viewport-before-fade-color');
                element.style.removeProperty('--tabs-viewport-after-fade-color');
            }");

    private static Task<bool> IsTabStripInViewportAsync(ILocator viewport) => viewport.EvaluateAsync<bool>(
        "element => { const rect = element.getBoundingClientRect(); return rect.bottom > 0 && rect.top < window.innerHeight; }");

    private static async Task ScrollTabStripOutOfViewAsync(IPage page, ILocator tabsViewport)
    {
        // The demo page can be shorter than the viewport, leaving nothing to scroll. Append a tall spacer so the
        // document always has enough room to push the tab strip completely above the top of the viewport.
        await page.EvaluateAsync(
            @"() => {
                let spacer = document.getElementById('tab-strip-test-spacer');
                if (!spacer) {
                    spacer = document.createElement('div');
                    spacer.id = 'tab-strip-test-spacer';
                    document.body.appendChild(spacer);
                }
                spacer.style.height = (window.innerHeight * 3) + 'px';
            }");

        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (!await IsTabStripInViewportAsync(tabsViewport))
                return;

            await page.EvaluateAsync("() => window.scrollBy(0, window.innerHeight)");
            await Task.Delay(50);
        }
    }

    private static async Task ScrollTabStripIntoViewAsync(ILocator tabsViewport)
    {
        await tabsViewport.EvaluateAsync("element => element.scrollIntoView({ block: 'center' })");

        for (var attempt = 0; attempt < 30 && !await IsTabStripInViewportAsync(tabsViewport); attempt++)
            await Task.Delay(50);
    }
}
