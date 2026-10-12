using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;

namespace Server.Tests.Tables.AdvancedTable;

[Collection<ServerTestCollection>]
public class FooterPaddingBottomTests(ServerFixture fixture)
{
    private const string Page = "/tables/advanced-table/footer";
    private const string RootSelector = ".advanced-table";
    private const string Property = "--vo-ui-bc-tables-footer-padding-bottom";

    [Fact]
    public async Task Should_keep_the_default_padding_when_the_property_is_not_set()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}{Page}");

            var paddingBottom = await FooterPaddingBottom.MeasureAsync(
                page, RootSelector, Property, setProperty: false, nest: false, "16px");

            paddingBottom.Should().Be("16px");
        });
    }

    [Fact]
    public async Task Should_apply_the_property_set_on_the_root_element()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}{Page}");

            var paddingBottom = await FooterPaddingBottom.MeasureAsync(
                page, RootSelector, Property, setProperty: true, nest: false, "0px");

            paddingBottom.Should().Be("0px");
        });
    }

    [Fact]
    public async Task Should_keep_the_default_padding_in_a_nested_table()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}{Page}");

            var paddingBottoms = await FooterPaddingBottom.MeasureAsync(
                page, RootSelector, Property, setProperty: true, nest: true, "0px 16px");

            paddingBottoms.Should().Be("0px 16px");
        });
    }
}
