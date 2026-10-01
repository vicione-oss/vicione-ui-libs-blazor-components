using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.CheckBox;

[Collection<ServerTestCollection>]
public class CheckBoxTests(ServerFixture fixture)
{
    [Fact]
    public async Task Should_not_trigger_container_click_when_checkbox_inside_is_clicked()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/check-box");

            var card = page.Locator(".clickable-card");
            var counter = card.GetByText("Card clicked");
            var checkbox = card.GetByRole(AriaRole.Checkbox);

            // The checkbox starts unchecked and the container has not been clicked yet.
            await Expect(counter).ToHaveTextAsync("Card clicked 0 time(s)");
            await Expect(checkbox).Not.ToBeCheckedAsync();

            // Clicking the checkbox toggles it but must not bubble to the container's click.
            await checkbox.ClickAsync();

            await Expect(checkbox).ToBeCheckedAsync();
            await Expect(counter).ToHaveTextAsync("Card clicked 0 time(s)");

            // Clicking the container (outside the checkbox) does increment the counter.
            await counter.ClickAsync();

            await Expect(counter).ToHaveTextAsync("Card clicked 1 time(s)");
            await Expect(checkbox).ToBeCheckedAsync();
        });
    }
}
