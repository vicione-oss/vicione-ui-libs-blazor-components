using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Popup;

[Collection<ServerTestCollection>]
public class PopupTests(ServerFixture fixture)
{
    private const int EscapeSettleDelay = 250;
    private const int FocusSettleDelay = 250;

    private static readonly TimeSpan s_focusTimeout = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task Should_close_popup_on_escape()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await Expect(popup).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task Should_not_close_popup_on_escape_when_disabled()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            await popup.Locator(".switch.close-on-escape").ClickAsync();

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await page.WaitForTimeoutAsync(EscapeSettleDelay);

            await Expect(popup).ToHaveCountAsync(1);
        });
    }

    [Fact]
    public async Task Should_not_close_popup_on_escape_reverting_text_box_input()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            var textBox = popup.Locator(".spin-edit").First.GetByRole(AriaRole.Textbox);
            var committedValue = await textBox.InputValueAsync();

            await textBox.ClickAsync();
            await textBox.PressSequentiallyAsync("123");

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await page.WaitForTimeoutAsync(EscapeSettleDelay);

            await Expect(popup).ToHaveCountAsync(1);
            await Expect(textBox).ToHaveValueAsync(committedValue);
        });
    }

    [Fact]
    public async Task Should_close_popup_on_escape_after_text_box_input_was_reverted()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            var textBox = popup.Locator(".spin-edit").First.GetByRole(AriaRole.Textbox);
            var committedValue = await textBox.InputValueAsync();

            await textBox.ClickAsync();
            await textBox.PressSequentiallyAsync("123");

            await page.Keyboard.PressAsync("Escape");

            await Expect(textBox).ToHaveValueAsync(committedValue);

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await Expect(popup).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task Should_not_close_popup_on_escape_clearing_tag_box_input()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            var tagInput = popup.Locator(".tag-box .tag-input");

            await tagInput.ClickAsync();
            await tagInput.PressSequentiallyAsync("draft");

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await page.WaitForTimeoutAsync(EscapeSettleDelay);

            await Expect(popup).ToHaveCountAsync(1);
            await Expect(tagInput).ToHaveValueAsync(string.Empty);
        });
    }

    [Fact]
    public async Task Should_close_popup_on_escape_from_empty_tag_box()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            await popup.Locator(".tag-box .tag-input").ClickAsync();

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await Expect(popup).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task Should_close_popup_on_escape_after_it_was_shown_again()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            await page.Keyboard.PressAsync("Escape");

            await Expect(popup).ToHaveCountAsync(0);

            // The dialog is created anew, so Escape has to be listened to on the new one.
            await page.Locator(".switch.popup-visible").ClickAsync();

            await Expect(popup).ToBeFocusedAsync();

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await Expect(popup).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task Should_spin_value_on_arrow_key_inside_popup()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            var popup = await ShowPopupAsync(page);

            // The spin edit for the width, starting at 640 with an interval of 10
            var textBox = popup.Locator(".spin-edit").Nth(1).GetByRole(AriaRole.Textbox);

            await textBox.ClickAsync();

            // Act
            // Listening for Escape must not re-render the popup content on other keys, which would undo
            // what the spin edit did with the same key press.
            await textBox.PressAsync("ArrowUp");

            // Assert
            await Expect(textBox).ToHaveValueAsync("650");
        });
    }

    [Fact]
    public async Task Should_move_focus_back_into_popup_with_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);

            // Act
            await page.Locator(".switch.popup-visible").FocusAsync();

            // Assert
            var activeElementClass = await WaitForActiveElementAsync(page, "first-popup");

            activeElementClass.Should().Contain("first-popup");
        });
    }

    [Fact]
    public async Task Should_leave_focus_outside_popup_without_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: false);

            // Act
            await page.Locator(".switch.popup-visible").FocusAsync();

            // Assert
            await page.WaitForTimeoutAsync(FocusSettleDelay);

            var activeElementClass = await GetActiveElementClassAsync(page);

            activeElementClass.Should().Contain("popup-visible");
        });
    }

    [Fact]
    public async Task Should_move_focus_into_topmost_popup_with_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);
            await ShowSecondPopupAsync(page, showBackdrop: true);

            // Act
            // The popup below the topmost one must not take the focus back from it.
            await page.Locator(".first-popup .switch.show-backdrop").FocusAsync();

            // Assert
            var activeElementClass = await WaitForActiveElementAsync(page, "second-popup");

            activeElementClass.Should().Contain("second-popup");
        });
    }

    [Fact]
    public async Task Should_leave_focus_in_topmost_popup_with_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);
            await ShowSecondPopupAsync(page, showBackdrop: true);

            // Act
            await page.Locator(".second-popup .switch.second-popup-close").FocusAsync();

            // Assert
            await page.WaitForTimeoutAsync(FocusSettleDelay);

            var activeElementClass = await GetActiveElementClassAsync(page);

            activeElementClass.Should().Contain("second-popup-close");
        });
    }

    [Fact]
    public async Task Should_leave_focus_in_popup_without_backdrop_above_popup_with_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);
            await ShowSecondPopupAsync(page, showBackdrop: false);

            // Act
            await page.Locator(".second-popup .switch.second-popup-close").FocusAsync();

            // Assert
            await page.WaitForTimeoutAsync(FocusSettleDelay);

            var activeElementClass = await GetActiveElementClassAsync(page);

            activeElementClass.Should().Contain("second-popup-close");
        });
    }

    [Fact]
    public async Task Should_allow_interaction_with_popup_without_backdrop_above_popup_with_backdrop()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);
            await ShowSecondPopupAsync(page, showBackdrop: false);

            // Act
            // The popup below must neither swallow the click nor take the focus the switch needs to be operated.
            await page.Locator(".second-popup .switch.second-popup-close").ClickAsync();

            // Assert
            await Expect(page.Locator(".second-popup")).ToHaveCountAsync(0);
            await Expect(page.Locator(".first-popup")).ToHaveCountAsync(1);
        });
    }

    [Fact]
    public async Task Should_move_focus_back_into_popup_with_backdrop_after_popup_above_is_closed()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await ShowPopupAsync(page, showBackdrop: true);
            await ShowSecondPopupAsync(page, showBackdrop: true);

            await page.Locator(".second-popup .switch.second-popup-close").ClickAsync();

            await Expect(page.Locator(".second-popup")).ToHaveCountAsync(0);

            // Act
            await page.Locator(".switch.popup-visible").FocusAsync();

            // Assert
            var activeElementClass = await WaitForActiveElementAsync(page, "first-popup");

            activeElementClass.Should().Contain("first-popup");
        });
    }

    private async Task<ILocator> ShowPopupAsync(IPage page, bool showBackdrop = false)
    {
        await page.GotoAsync($"{fixture.ServerAddress}/popup");

        await page.Locator(".switch.popup-visible").ClickAsync();

        var popup = page.Locator(".modal-dialog.first-popup");

        // The popup takes the focus once it has rendered, so a key pressed before would not reach it.
        await Expect(popup).ToBeFocusedAsync();

        if (showBackdrop)
            await popup.Locator(".switch.show-backdrop").ClickAsync();

        return popup;
    }

    private static async Task ShowSecondPopupAsync(IPage page, bool showBackdrop)
    {
        if (showBackdrop)
            await page.Locator(".first-popup .switch.second-popup-show-backdrop").ClickAsync();

        await page.Locator(".first-popup .switch.second-popup-visible").ClickAsync();
    }

    private static Task<string> GetActiveElementClassAsync(IPage page)
        => page.EvaluateAsync<string>("() => document.activeElement?.className ?? ''");

    private static async Task<string> WaitForActiveElementAsync(IPage page, string cssClass)
    {
        var deadline = DateTime.UtcNow + s_focusTimeout;

        var activeElementClass = await GetActiveElementClassAsync(page);

        while (!activeElementClass.Contains(cssClass, StringComparison.Ordinal) && DateTime.UtcNow < deadline)
            activeElementClass = await GetActiveElementClassAsync(page);

        return activeElementClass;
    }
}
