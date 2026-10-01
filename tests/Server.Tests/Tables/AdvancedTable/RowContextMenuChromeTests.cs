using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.AdvancedTable;

// Browser-only behavior of the row context menu. Blazor applies `@oncontextmenu:preventDefault` and
// `@oncontextmenu:stopPropagation` from its own JavaScript dispatch, so neither marker ever reaches bUnit's
// rendered markup — the unit tests can assert the parameters that feed them and nothing beyond that.
[Collection<ServerTestCollection>]
public class RowContextMenuChromeTests(ServerFixture fixture)
{
    // Records whether the row handler canceled the browser's default action. Blazor registers its own
    // document-level listener while the circuit starts, so it dispatches to the row before this listener
    // runs and defaultPrevented already carries the outcome by then.
    private const string RecordContextMenuOutcomeScript = """
        () => {
            window.contextMenuDefaultPrevented = null;
            document.addEventListener('contextmenu', event => {
                window.contextMenuDefaultPrevented = event.defaultPrevented;
            });
        }
        """;

    [Fact]
    public async Task Should_suppress_the_native_menu_when_a_row_is_right_clicked()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GoToRowContextMenuPageAsync(page);

            await page.Locator("tbody tr").First.ClickAsync(new() { Button = MouseButton.Right });

            // The custom menu opening is the visible half; the browser's own menu staying away is the half
            // only defaultPrevented can report, because native chrome is not in the DOM.
            await Expect(page.Locator(".context-menu")).ToBeVisibleAsync();
            Assert.True(await ReadContextMenuOutcomeAsync(page));
        });
    }

    [Fact]
    public async Task Should_leave_the_native_menu_alone_for_cell_content_that_opts_out()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GoToRowContextMenuPageAsync(page);

            // The input carries @oncontextmenu:stopPropagation, so Blazor never walks up to the row and the
            // row's preventDefault never applies — the browser keeps its cut/copy/paste menu.
            await page.Locator("tbody tr .native-context-menu-input").First
                .ClickAsync(new() { Button = MouseButton.Right });

            Assert.False(await ReadContextMenuOutcomeAsync(page));
            await Expect(page.Locator(".context-menu")).Not.ToBeVisibleAsync();
        });
    }

    private async Task GoToRowContextMenuPageAsync(IPage page)
    {
        await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/rows/context-menu");

        // The rows must be interactive before the listener is attached, otherwise the click lands on markup
        // whose circuit has not wired its handlers up yet and nothing cancels anything.
        await Expect(page.Locator("tbody tr").First).ToBeVisibleAsync();

        await page.EvaluateAsync(RecordContextMenuOutcomeScript);
    }

    private static async Task<bool> ReadContextMenuOutcomeAsync(IPage page)
    {
        var outcome = await page.EvaluateAsync<bool?>("() => window.contextMenuDefaultPrevented");

        Assert.NotNull(outcome);

        return outcome.Value;
    }
}
