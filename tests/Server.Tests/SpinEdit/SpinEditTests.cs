using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.SpinEdit;

[Collection<ServerTestCollection>]
public partial class SpinEditTests(ServerFixture fixture)
{
    private const string ScrollableContainerSelector = ".scrollable-container";

    // The sample layout scrolls this element rather than the document
    private const string PageScrollContainerSelector = ".body-container";

    [Fact]
    public async Task Should_revert_wrong_input_to_value_applicable()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            var firstSpinEdit = page.Locator(".spin-edit").First;
            var innerTextBox = firstSpinEdit.GetByRole(AriaRole.Textbox);

            // Remember the initial input value
            var initialInputValue = await innerTextBox.InputValueAsync();

            // Enter "foo" and remove focus
            await innerTextBox.ClickAsync();
            await innerTextBox.PressSequentiallyAsync("foo");
            await page.GetByRole(AriaRole.Main).ClickAsync();

            // Focus again, enter "foo" and remove focus
            await innerTextBox.ClickAsync();
            await innerTextBox.PressSequentiallyAsync("foo");
            await page.GetByRole(AriaRole.Main).ClickAsync();

            // Assert
            await Expect(innerTextBox).ToHaveValueAsync(initialInputValue);
        });
    }

    [Theory]
    [InlineData("ArrowUp", "130")]
    [InlineData("ArrowDown", "110")]
    public async Task Should_spin_value_on_arrow_key(string key, string expectedValue)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            // The bound spin edit, starting at 120 with an interval of 10
            var innerTextBox = page.Locator(".spin-edit").Nth(1).GetByRole(AriaRole.Textbox);

            await innerTextBox.ClickAsync();

            // Act
            await innerTextBox.PressAsync(key);

            // Assert
            await Expect(innerTextBox).ToHaveValueAsync(expectedValue);
        });
    }

    [Fact]
    public async Task Should_spin_entered_value_on_arrow_key()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            // The bound spin edit, starting at 120 with an interval of 10
            var innerTextBox = page.Locator(".spin-edit").Nth(1).GetByRole(AriaRole.Textbox);

            await innerTextBox.ClickAsync();
            await innerTextBox.SelectTextAsync();
            await innerTextBox.PressSequentiallyAsync("50");

            // Act
            // The entered value is not committed yet, which is when the text box has something to revert
            // on Escape. Arrow keys must still reach the spin edit around it.
            await innerTextBox.PressAsync("ArrowUp");

            // Assert
            await Expect(innerTextBox).ToHaveValueAsync("60");
        });
    }

    [Theory]
    [InlineData(-100, "130")]
    [InlineData(100, "110")]
    public async Task Should_spin_value_on_wheel_when_focused(int deltaY, string expectedValue)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            // The bound spin edit, starting at 120 with an interval of 10
            var spinEdit = page.Locator(".spin-edit").Nth(1);
            var innerTextBox = spinEdit.GetByRole(AriaRole.Textbox);

            await innerTextBox.ClickAsync();
            await Expect(spinEdit).ToHaveClassAsync(FocusedCssClass());

            // Act
            await page.Mouse.WheelAsync(0, deltaY);

            // Assert
            await Expect(innerTextBox).ToHaveValueAsync(expectedValue);
        });
    }

    [Fact]
    public async Task Should_neither_spin_value_nor_take_focus_on_wheel_when_not_focused()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            // The bound spin edit, starting at 120 with an interval of 10
            var spinEdit = page.Locator(".spin-edit").Nth(1);
            var innerTextBox = spinEdit.GetByRole(AriaRole.Textbox);

            await innerTextBox.HoverAsync();

            // Act
            await page.Mouse.WheelAsync(0, -100);

            // Assert
            await Expect(innerTextBox).Not.ToBeFocusedAsync();
            await Expect(innerTextBox).ToHaveValueAsync("120");
        });
    }

    [Fact]
    public async Task Should_scroll_container_on_wheel_when_not_focused()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            var innerTextBox = page.Locator($"{ScrollableContainerSelector} .spin-edit").GetByRole(AriaRole.Textbox);

            await innerTextBox.ScrollIntoViewIfNeededAsync();
            await AssertCanScrollDownAsync(page, ScrollableContainerSelector);

            var initialInputValue = await innerTextBox.InputValueAsync();
            var initialScrollTop = await GetScrollTopAsync(page, ScrollableContainerSelector);

            await innerTextBox.HoverAsync();

            // Act
            await page.Mouse.WheelAsync(0, 50);

            // Assert
            await WaitForScrollTopToExceedAsync(page, ScrollableContainerSelector, initialScrollTop);
            await Expect(innerTextBox).ToHaveValueAsync(initialInputValue);
        });
    }

    [Fact]
    public async Task Should_not_scroll_container_on_wheel_when_focused()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            var spinEdit = page.Locator($"{ScrollableContainerSelector} .spin-edit");
            var innerTextBox = spinEdit.GetByRole(AriaRole.Textbox);

            await innerTextBox.ScrollIntoViewIfNeededAsync();
            await AssertCanScrollDownAsync(page, ScrollableContainerSelector);

            await innerTextBox.ClickAsync();
            await Expect(spinEdit).ToHaveClassAsync(FocusedCssClass());

            var initialInputValue = await innerTextBox.InputValueAsync();
            var initialScrollTop = await GetScrollTopAsync(page, ScrollableContainerSelector);

            // Act
            await page.Mouse.WheelAsync(0, 50);

            // Assert
            // The changed value shows that the wheel was handled, so a scroll would have happened by now
            await Expect(innerTextBox).Not.ToHaveValueAsync(initialInputValue);
            (await GetScrollTopAsync(page, ScrollableContainerSelector)).Should().Be(initialScrollTop);
        });
    }

    [Fact]
    public async Task Should_scroll_container_on_wheel_again_after_focus_left()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            var spinEdit = page.Locator($"{ScrollableContainerSelector} .spin-edit");
            var innerTextBox = spinEdit.GetByRole(AriaRole.Textbox);

            await innerTextBox.ScrollIntoViewIfNeededAsync();
            await AssertCanScrollDownAsync(page, ScrollableContainerSelector);

            await innerTextBox.ClickAsync();
            await Expect(spinEdit).ToHaveClassAsync(FocusedCssClass());

            await innerTextBox.BlurAsync();
            await Expect(spinEdit).Not.ToHaveClassAsync(FocusedCssClass());

            var initialInputValue = await innerTextBox.InputValueAsync();
            var initialScrollTop = await GetScrollTopAsync(page, ScrollableContainerSelector);

            await innerTextBox.HoverAsync();

            // Act
            await page.Mouse.WheelAsync(0, 50);

            // Assert
            await WaitForScrollTopToExceedAsync(page, ScrollableContainerSelector, initialScrollTop);
            await Expect(innerTextBox).ToHaveValueAsync(initialInputValue);
        });
    }

    [Theory]
    [InlineData("Read-only")]
    [InlineData("Disabled")]
    public async Task Should_scroll_page_on_wheel_when_spin_edit_cannot_receive_input(string heading)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/spin-edit");

            var innerTextBox = page.Locator($"h3:text-is('{heading}') + .spin-edit").GetByRole(AriaRole.Textbox);

            await innerTextBox.ScrollIntoViewIfNeededAsync();
            await AssertCanScrollDownAsync(page, PageScrollContainerSelector);

            var initialInputValue = await innerTextBox.InputValueAsync();
            var initialScrollTop = await GetScrollTopAsync(page, PageScrollContainerSelector);

            await innerTextBox.HoverAsync();

            // Act
            await page.Mouse.WheelAsync(0, 50);

            // Assert
            await WaitForScrollTopToExceedAsync(page, PageScrollContainerSelector, initialScrollTop);
            await Expect(innerTextBox).ToHaveValueAsync(initialInputValue);
        });
    }

    [GeneratedRegex(@"\bfocused\b")]
    private static partial Regex FocusedCssClass();

    private static Task<double> GetScrollTopAsync(IPage page, string selector)
        => page.EvaluateAsync<double>("selector => document.querySelector(selector).scrollTop", selector);

    // Without room to scroll, a scroll top that stays put proves nothing and one that should grow can't
    private static async Task AssertCanScrollDownAsync(IPage page, string selector)
    {
        var canScrollDown = await page.EvaluateAsync<bool>(@"selector => {
            const element = document.querySelector(selector);
            return element.scrollTop + element.clientHeight < element.scrollHeight;
        }", selector);

        canScrollDown.Should().BeTrue("the container must be able to scroll for the wheel to prove anything");
    }

    // The wheel returns before the browser has scrolled, which it may also do smoothly over several frames.
    // A short timeout lets a regression fail fast, reporting the scroll top it ended up with.
    private static async Task WaitForScrollTopToExceedAsync(IPage page, string selector, double scrollTop)
    {
        try
        {
            await page.WaitForFunctionAsync(
                "([selector, scrollTop]) => document.querySelector(selector).scrollTop > scrollTop",
                new object[] { selector, scrollTop }, new() { Timeout = 5_000 });
        }
        catch (TimeoutException)
        {
            (await GetScrollTopAsync(page, selector)).Should()
                .BeGreaterThan(scrollTop, "the wheel should have scrolled {0}", selector);
        }
    }
}
