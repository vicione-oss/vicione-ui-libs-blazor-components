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
}
