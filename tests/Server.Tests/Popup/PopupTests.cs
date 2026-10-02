using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Popup;

[Collection<ServerTestCollection>]
public class PopupTests(ServerFixture fixture)
{
    private const int EscapeSettleDelay = 250;

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

    private async Task<ILocator> ShowPopupAsync(IPage page)
    {
        await page.GotoAsync($"{fixture.ServerAddress}/popup");

        await page.Locator(".switch.popup-visible").ClickAsync();

        var popup = page.Locator(".modal-dialog");

        // The popup takes the focus once it has rendered, so a key pressed before would not reach it.
        await Expect(popup).ToBeFocusedAsync();

        return popup;
    }
}
