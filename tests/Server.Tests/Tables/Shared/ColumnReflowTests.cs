using System.Text.Json;
using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Tables.Shared;

// The column widths are computed in TypeScript against a real layout, so this is the only tier that can check
// the arithmetic. The container width depends on the browser's chrome, so the widths are asserted as
// properties of the result rather than as literals.
[Collection<ServerTestCollection>]
public class ColumnReflowTests(ServerFixture fixture)
{
    private const string ReflowPageUrl = "/tables/advanced-table/reflow";

    // The SimpleTable page mirrors this one sample for sample, so the same selectors address both.
    private const string SimpleTableReflowPageUrl = "/tables/simple-table/reflow";

    // The one sample whose container is uncapped: the capped ones only narrow below their own max-width, which
    // makes them useless for a viewport test.
    private const string FullWidthTableSelector = ".reflow-container.full-width table.inner-table";

    private const string AllFlexTableSelector = ".reflow-container.all-flex table.inner-table";

    // The sample where Id declares Width 100, Key floors at 200 and the other two are plain flex.
    private const string MixedTableSelector = ".reflow-container.mixed table.inner-table";

    private const string AllFixedTableSelector = ".reflow-container.all-fixed table.inner-table";

    // How far the columns may fall short of their container before the gap counts as one. The flex columns are
    // shared out at full precision and the browser then snaps each one to its own layout unit — a sixty-fourth
    // of a pixel in Chromium — so the total lands a fraction under the container rather than dead on it. Half a
    // pixel is the threshold that matters: below it nothing can be drawn, and so nothing can be seen.
    private const double SubPixelTolerance = 0.5;

    [Fact]
    public async Task Should_not_move_the_columns_after_the_table_is_first_painted()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            // An odd width on purpose: it leaves the uncapped sample a container its four flex columns divide
            // into a half pixel each, which is the case the re-flow used to round its way out of. A width they
            // happen to divide exactly would hide this.
            await page.SetViewportSizeAsync(802, 900);

            await RecordColumnEdgesAsync(page, FullWidthTableSelector);

            // Act: arriving at a page builds a table with no width of its own, so the browser lays the flex
            // columns out first and the re-flow that follows it is what can move them.
            await page.GotoAsync($"{fixture.ServerAddress}{ReflowPageUrl}");

            await WaitForColumnsToFillTheContainerAsync(page, FullWidthTableSelector);

            // Assert
            var layouts = await GetRecordedColumnEdgesAsync(page);

            var painted = layouts[0];
            var settled = layouts[^1];

            Assert.Equal(painted.Length, settled.Length);

            for (var edge = 0; edge < painted.Length; edge++)
            {
                Assert.True(Math.Abs(painted[edge] - settled[edge]) <= SubPixelTolerance,
                    $"column edge {edge} moved from {painted[edge]} to {settled[edge]} after the first paint; " +
                    $"all edges went {string.Join(", ", painted)} -> {string.Join(", ", settled)}");
            }
        });
    }

    [Fact]
    public async Task Should_split_the_container_equally_among_flex_columns()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            var widths = await GetColumnWidthsAsync(page, AllFlexTableSelector);
            var containerWidth = await GetContainerWidthAsync(page, AllFlexTableSelector);

            Assert.Equal(3, widths.Count);

            AssertFillsTheContainer(containerWidth, widths);
            Assert.All(widths, width => Assert.True(Math.Abs(width - (containerWidth / 3.0)) < 2,
                $"{width} is not within a pixel of an equal third of {containerWidth}"));
        });
    }

    [Fact]
    public async Task Should_re_split_the_space_when_the_container_narrows()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            var wideWidths = await GetColumnWidthsAsync(page, AllFlexTableSelector);

            // Act
            await NarrowTheContainerAsync(page, AllFlexTableSelector);

            // Assert
            var narrowContainerWidth = await GetContainerWidthAsync(page, AllFlexTableSelector);
            var narrowWidths = await GetColumnWidthsAsync(page, AllFlexTableSelector);

            Assert.True(narrowWidths.Sum() < wideWidths.Sum(),
                $"columns did not re-flow: {narrowWidths.Sum()} is not narrower than {wideWidths.Sum()}");

            AssertFillsTheContainer(narrowContainerWidth, narrowWidths);
        });
    }

    [Fact]
    public async Task Should_re_split_the_space_when_the_window_is_resized()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.SetViewportSizeAsync(1600, 900);
            await GotoReflowPageAsync(page);

            var wideWidth = await GetTotalColumnWidthAsync(page, FullWidthTableSelector);

            // Act
            await page.SetViewportSizeAsync(900, 900);

            // Assert
            await WaitForColumnsToFillTheContainerAsync(page, FullWidthTableSelector);

            var narrowWidth = await GetTotalColumnWidthAsync(page, FullWidthTableSelector);

            Assert.True(narrowWidth < wideWidth,
                $"columns did not follow the window: {narrowWidth} against {wideWidth}");
        });
    }

    [Fact]
    public async Task Should_re_flow_a_simple_table_the_same_way()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await page.SetViewportSizeAsync(1600, 900);
            await page.GotoAsync($"{fixture.ServerAddress}{SimpleTableReflowPageUrl}");

            await WaitForColumnsToFillTheContainerAsync(page, FullWidthTableSelector);

            var wideWidth = await GetTotalColumnWidthAsync(page, FullWidthTableSelector);

            // Act
            await page.SetViewportSizeAsync(900, 900);

            // Assert
            await WaitForColumnsToFillTheContainerAsync(page, FullWidthTableSelector);

            var narrowWidth = await GetTotalColumnWidthAsync(page, FullWidthTableSelector);

            Assert.True(narrowWidth < wideWidth,
                $"the wrapped table did not re-flow: {narrowWidth} against {wideWidth}");
        });
    }

    [Fact]
    public async Task Should_leave_a_column_that_declares_a_width_where_it_is()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            await Expect(page.Locator($"{MixedTableSelector} col[data-column-id='Id']"))
                .ToHaveAttributeAsync("style", "width: 100px");

            // Act
            await NarrowTheContainerAsync(page, MixedTableSelector);

            // Assert
            await Expect(page.Locator($"{MixedTableSelector} col[data-column-id='Id']"))
                .ToHaveAttributeAsync("style", "width: 100px");
        });
    }

    [Fact]
    public async Task Should_clamp_a_flex_column_at_its_minimum_width_and_overflow_the_container()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            // Act: the narrow container is capped under the 400px this sample cannot shrink below — Id's
            // declared 100, Key's minimum 200 and 50 each for the last two
            await NarrowTheContainerAsync(page, MixedTableSelector);

            // Assert
            await Expect(page.Locator($"{MixedTableSelector} col[data-column-id='Key']"))
                .ToHaveAttributeAsync("style", "width: 200px");

            var widths = await GetColumnWidthsAsync(page, MixedTableSelector);
            var containerWidth = await GetContainerWidthAsync(page, MixedTableSelector);

            Assert.True(widths.Sum() > containerWidth,
                $"expected the clamped columns to overflow {containerWidth}, got {widths.Sum()}");
        });
    }

    [Fact]
    public async Task Should_keep_its_own_width_when_every_column_is_fixed()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            var containerWidth = await GetContainerWidthAsync(page, AllFixedTableSelector);

            // Assert
            var widths = await GetColumnWidthsAsync(page, AllFixedTableSelector);

            Assert.Equal([100, 160, 160], widths);
            Assert.True(containerWidth > widths.Sum(),
                "the sample container is expected to be wider than the fixed columns");
        });
    }

    [Fact]
    public async Task Should_pin_only_the_dragged_column_and_go_on_re_flowing_the_others()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await GotoReflowPageAsync(page);

            var draggedColumn = page.Locator($"{MixedTableSelector} col[data-column-id='Value']");
            var neighbor = page.Locator($"{MixedTableSelector} col[data-column-id='Timestamp']");

            var widthBeforeDrag = await GetColumnWidthAsync(page, MixedTableSelector, "Value");
            var neighborWidthBeforeDrag = await GetColumnWidthAsync(page, MixedTableSelector, "Timestamp");

            // Act
            await DragResizeHandleAsync(page, MixedTableSelector, "Value", 80);

            await Expect(draggedColumn).ToHaveAttributeAsync("style", $"width: {widthBeforeDrag + 80}px");

            await Expect(draggedColumn).Not.ToHaveAttributeAsync("data-flex", "");
            await Expect(neighbor).ToHaveAttributeAsync("data-flex", "");

            await NarrowTheContainerAsync(page, MixedTableSelector);

            // Assert
            await Expect(draggedColumn).ToHaveAttributeAsync("style", $"width: {widthBeforeDrag + 80}px");

            var neighborWidthAfterReflow = await GetColumnWidthAsync(page, MixedTableSelector, "Timestamp");

            Assert.True(neighborWidthAfterReflow < neighborWidthBeforeDrag,
                $"the neighbor stopped re-flowing: {neighborWidthAfterReflow} against {neighborWidthBeforeDrag}");
        });
    }

    private async Task GotoReflowPageAsync(IPage page)
    {
        await page.GotoAsync($"{fixture.ServerAddress}{ReflowPageUrl}");

        await WaitForColumnsToFillTheContainerAsync(page, AllFlexTableSelector);
        await WaitForColumnsToFillTheContainerAsync(page, FullWidthTableSelector);
    }

    private static async Task<double> GetTotalColumnWidthAsync(IPage page, string tableSelector)
        => (await GetColumnWidthsAsync(page, tableSelector)).Sum();

    // The first layout lands only once the module has attached and its observer has delivered a callback.
    // Only for a table nothing clamps: one holding a column at its minimum overflows its container on purpose.
    private static async Task WaitForColumnsToFillTheContainerAsync(IPage page, string tableSelector)
        => await Assert_ThatAsync(async () =>
        {
            var widths = await GetColumnWidthsAsync(page, tableSelector);
            var containerWidth = await GetContainerWidthAsync(page, tableSelector);

            Assert.NotEmpty(widths);
            AssertFillsTheContainer(containerWidth, widths);
        });

    // Installs a recorder that keeps every distinct set of column edges the table is drawn with, from the very
    // first frame it exists in. Nothing else can see the layout the browser gives the flex columns before the
    // module has attached: by the time a test can ask, the re-flow has long replaced it.
    private static async Task RecordColumnEdgesAsync(IPage page, string tableSelector)
        => await page.AddInitScriptAsync($$"""
            window.__columnEdges = [];

            const sample = () => {
                const table = document.querySelector({{JsonSerializer.Serialize(tableSelector)}});

                if (table) {
                    const edges = [...table.querySelectorAll('colgroup col')]
                        .map(col => col.getBoundingClientRect().right);

                    const previous = window.__columnEdges.at(-1);

                    if (edges.length > 0 && (!previous || previous.join() !== edges.join()))
                        window.__columnEdges.push(edges);
                }

                requestAnimationFrame(sample);
            };

            requestAnimationFrame(sample);
            """);

    private static async Task<double[][]> GetRecordedColumnEdgesAsync(IPage page)
        => await page.EvaluateAsync<double[][]>("() => window.__columnEdges");

    private static void AssertFillsTheContainer(double containerWidth, IReadOnlyCollection<double> widths)
        => Assert.True(Math.Abs(containerWidth - widths.Sum()) <= SubPixelTolerance,
            $"the columns add up to {widths.Sum()}, which does not fill a container of {containerWidth}");

    private static async Task NarrowTheContainerAsync(IPage page, string tableSelector)
    {
        var containerWidthBefore = await GetContainerWidthAsync(page, tableSelector);
        var tableWidthBefore = await GetTableWidthAsync(page, tableSelector);

        await page.Locator(".options .option .switch").First.ClickAsync();

        // Both widths having moved is the general settle signal: the layout writes the table width in the same
        // synchronous block as the column widths.
        await Assert_ThatAsync(async () =>
        {
            var containerWidth = await GetContainerWidthAsync(page, tableSelector);

            Assert.True(containerWidth < containerWidthBefore,
                $"the container has not narrowed yet: still {containerWidth}");

            Assert.NotEqual(tableWidthBefore, await GetTableWidthAsync(page, tableSelector));
        });
    }

    // Measured at full precision. Rounding each column to a whole pixel first would throw away exactly what
    // these tests are about: a flex column is a fraction of the container, and three of them rounded down no
    // longer add up to it.
    private static async Task<List<double>> GetColumnWidthsAsync(IPage page, string tableSelector)
    {
        var widths = await page.Locator($"{tableSelector} colgroup col").EvaluateAllAsync<double[]>(
            "elements => elements.map(element => element.getBoundingClientRect().width)");

        return [.. widths];
    }

    // Whole pixels, unlike the others: this one is only asked for a column a drag has pinned, and a drag writes
    // the width it measured with offsetWidth.
    private static async Task<int> GetColumnWidthAsync(IPage page, string tableSelector, string columnId)
    {
        var width = await page.Locator($"{tableSelector} colgroup col[data-column-id='{columnId}']")
            .EvaluateAsync<double>("element => element.getBoundingClientRect().width");

        return (int)Math.Round(width);
    }

    private static async Task<double> GetTableWidthAsync(IPage page, string tableSelector)
        => await page.Locator(tableSelector).EvaluateAsync<double>(
            "element => element.getBoundingClientRect().width");

    // Read through a one-shot ResizeObserver, which is the component's own source of truth and reports the
    // content box at full precision. clientWidth would be a whole number, and so a container up to half a pixel
    // away from the one the columns were actually shared out across.
    private static async Task<double> GetContainerWidthAsync(IPage page, string tableSelector)
        => await page.Locator(tableSelector).EvaluateAsync<double>(
            """
            element => new Promise(resolve => {
                const observer = new ResizeObserver(entries => {
                    observer.disconnect();
                    resolve(entries.at(-1).contentRect.width);
                });

                observer.observe(element.closest('.inner-table-container'));
            })
            """);

    private static async Task DragResizeHandleAsync(IPage page, string tableSelector, string columnId, int deltaX)
    {
        var handle = page.Locator($"{tableSelector} th[data-column-id='{columnId}'] .resize-handle").First;

        // The mouse is driven by viewport coordinates and BoundingBoxAsync reports them without scrolling
        // anything into view, so a handle below the fold would be measured where the mouse cannot reach it.
        await handle.ScrollIntoViewIfNeededAsync();

        var handleBox = await handle.BoundingBoxAsync();

        Assert.NotNull(handleBox);

        var startX = handleBox.X + (handleBox.Width / 2);
        var startY = handleBox.Y + (handleBox.Height / 2);

        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(startX + deltaX, startY, new MouseMoveOptions { Steps = 10 });
        await page.Mouse.UpAsync();
    }

    // Retries until the assertion stops throwing: the widths land through a ResizeObserver callback and a
    // debounced interop commit, and Playwright's own auto-waiting only covers locator assertions.
    private static async Task Assert_ThatAsync(Func<Task> assertion)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (true)
        {
            try
            {
                await assertion();

                return;
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(100);
            }
        }
    }
}
