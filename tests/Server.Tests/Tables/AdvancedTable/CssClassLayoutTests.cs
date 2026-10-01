using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.AdvancedTable;

// Sizing a table through CssClass only shows in real layout; bUnit renders markup without applying any CSS.
[Collection<ServerTestCollection>]
public class CssClassLayoutTests(ServerFixture fixture)
{
    [Fact]
    public async Task Should_fill_the_flex_column_and_virtualize_when_sized_through_css_class()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/loading-mode");

            var table = page.Locator(".filling-table");
            await Expect(table.Locator("tbody tr").First).ToBeVisibleAsync();

            // The sample's flex column is 400px high with 60px above the table, leaving 340px for the table.
            var tableBox = await table.BoundingBoxAsync();
            tableBox.Should().NotBeNull();
            tableBox!.Height.Should().BeApproximately(340, 1);

            // Of the sample's 100 items only those in view are rendered, which proves the height was limited.
            var renderedRowCount = await table.Locator("tbody tr").CountAsync();
            renderedRowCount.Should().BeLessThan(100);
        });
    }
}
