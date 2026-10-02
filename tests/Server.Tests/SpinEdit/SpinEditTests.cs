using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.SpinEdit;

[Collection<ServerTestCollection>]
public class SpinEditTests(ServerFixture fixture)
{
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
}
