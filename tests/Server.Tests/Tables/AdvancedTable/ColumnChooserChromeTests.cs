using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.AdvancedTable;

// Browser-only behavior of the column chooser popup. The popup stops click propagation, and stopPropagation
// only actually stops anything in a real event pipeline — bUnit dispatches straight to the handler it finds, so
// a missing stopPropagation looks identical there.
[Collection<ServerTestCollection>]
public partial class ColumnChooserChromeTests(ServerFixture fixture)
{
    [GeneratedRegex(@"(^|\s)active(\s|$)")]
    private static partial Regex ActiveToggleButtonClass { get; }

    [Fact]
    public async Task Should_keep_the_popup_open_when_its_own_chrome_is_clicked()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/column-chooser");

            var toggleButton = page.Locator(".column-chooser-toggle .toolbar-button");
            var popup = page.Locator(".column-chooser-popup");

            await toggleButton.ClickAsync();
            await Expect(popup).ToBeVisibleAsync();

            // The toggle sits inside a Toolbar, whose items carry click handlers of their own, and the popup
            // renders as a sibling of the button that opened it. A click escaping the popup would reach those
            // handlers and close the popup out from under whoever is reading the column list.
            await popup.Locator(".header-text").ClickAsync();

            await Expect(popup).ToBeVisibleAsync();

            // The button mirrors the popup state through ColumnChooserButtonContext, so it must still read open.
            await Expect(toggleButton).ToHaveClassAsync(ActiveToggleButtonClass);
        });
    }

    [Fact]
    public async Task Should_keep_the_popup_open_while_a_column_is_hidden_from_it()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/advanced-table/column-chooser");

            var headers = page.Locator("table.inner-table th[data-column-id]");
            await Expect(headers).ToHaveCountAsync(4);

            await page.Locator(".column-chooser-toggle .toolbar-button").ClickAsync();

            var popup = page.Locator(".column-chooser-popup");
            await Expect(popup).ToBeVisibleAsync();

            // Unchecking has to take the column out of the table and leave the popup open, so more than one
            // column can be hidden without reopening the popup between clicks.
            await popup.GetByRole(AriaRole.Checkbox).First.ClickAsync();

            await Expect(headers).ToHaveCountAsync(3);
            await Expect(popup).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Should_list_at_most_the_maximum_visible_rows_and_scroll_to_the_others()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/simple-table/column-chooser");

            await page.Locator(".column-chooser-toggle .toolbar-button").ClickAsync();

            var popup = page.Locator(".column-chooser-popup");
            await Expect(popup).ToBeVisibleAsync();

            // The sample sets MaximumVisibleRowCount to 2 for its 3 chooser columns. Unlimited, a long column
            // list stretched the popup past its dialog and scrolled the whole dialog out of view.
            var checkBoxes = popup.GetByRole(AriaRole.Checkbox);
            await Expect(checkBoxes).ToHaveCountAsync(3);

            await Expect(checkBoxes.Nth(1)).ToBeInViewportAsync(new() { Ratio = 1 });
            await Expect(checkBoxes.Nth(2)).Not.ToBeInViewportAsync();

            // The remaining columns must stay reachable by scrolling the list.
            await checkBoxes.Last.ScrollIntoViewIfNeededAsync();

            await Expect(checkBoxes.Last).ToBeInViewportAsync(new() { Ratio = 1 });
            await Expect(checkBoxes.First).Not.ToBeInViewportAsync();
        });
    }

    [Fact]
    public async Task Should_not_scroll_the_column_list_when_all_columns_fit()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tables/simple-table/column-chooser");

            await page.Locator(".column-chooser-toggle .toolbar-button").ClickAsync();

            var popup = page.Locator(".column-chooser-popup");
            await Expect(popup).ToBeVisibleAsync();

            // The sample limits its 3 chooser columns to 2 rows. A limit of exactly 3 is what ColumnChooserToggle
            // renders for MaximumVisibleRowCount="3", the tightest one that fits every column.
            await popup.EvaluateAsync("p => p.style.setProperty('--column-chooser-maximum-visible-row-count', '3')");

            // The CheckBox icons are larger than their box. Their transparent overhang on the last row used to count
            // as overflow and gave the list a scrollbar although every column was shown.
            var overflow = await popup.Locator(".popup-header-body-layout > .body")
                .EvaluateAsync<int>("body => body.scrollHeight - body.clientHeight");

            Assert.True(overflow <= 0, $"The column list overflows by {overflow}px");
        });
    }
}
