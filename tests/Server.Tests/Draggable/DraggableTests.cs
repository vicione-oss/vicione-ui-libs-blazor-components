using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace Server.Tests.Draggable;

[Collection<ServerTestCollection>]
public class DraggableTests(ServerFixture fixture)
{
    // Grab close to the draggable's top-left corner so the drag ghost host's top-left (used for dropzone
    // hit-testing) tracks the pointer, keeping wide elements like table rows inside the dropzone.
    private const int GrabOffset = 8;

    [Fact]
    public async Task Should_drop_shape_with_default_drag_clone_onto_dropzone()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var shapeA = page.Locator(".shape", new() { HasTextString = "Shape A" });

            await DragOntoDropzoneAsync(page, shapeA, dropzoneIndex: 0, whileDragging: async p =>
            {
                // The default drag clone renders a copy of the dragged shape, so the drag ghost carries the
                // shape's own markup and not any custom drag ghost content.
                var ghostHtml = await GetDragGhostHtmlAsync(p);
                Assert.Contains("Shape A", ghostHtml, StringComparison.Ordinal);
                Assert.DoesNotContain("ticker-container", ghostHtml, StringComparison.Ordinal);
            });

            await AssertLastDropAsync(page, "Shape A", "Dropzone 1");
        });
    }

    [Fact]
    public async Task Should_drop_shape_with_time_ticker_drag_ghost_onto_dropzone()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var shapeB = page.Locator(".shape", new() { HasTextString = "Shape B" });

            await DragOntoDropzoneAsync(page, shapeB, dropzoneIndex: 1, whileDragging: async p =>
            {
                // Shape B supplies the TimeTickerDragGhost, so the drag ghost shows the ticker markup
                // instead of a plain clone of the shape.
                var ghostHtml = await GetDragGhostHtmlAsync(p);
                Assert.Contains("ticker-container", ghostHtml, StringComparison.Ordinal);
                Assert.Contains("ticker-time", ghostHtml, StringComparison.Ordinal);
            });

            await AssertLastDropAsync(page, "Shape B", "Dropzone 2");
        });
    }

    [Fact]
    public async Task Should_show_time_ticker_drag_ghost_cursor_while_dragging()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var shapeB = page.Locator(".shape", new() { HasTextString = "Shape B" });

            await DragOntoDropzoneAsync(page, shapeB, dropzoneIndex: 1,
                whileDragging: p => WaitForDragCursorAsync(p, "not-allowed"),
                overDropzone: p => WaitForDragCursorAsync(p, "alias"));
        });
    }

    [Fact]
    public async Task Should_keep_document_cursor_for_default_drag_clone()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var shapeA = page.Locator(".shape", new() { HasTextString = "Shape A" });

            await DragOntoDropzoneAsync(page, shapeA, dropzoneIndex: 0,
                whileDragging: async p => Assert.Equal("auto", await GetDragCursorAsync(p)),
                overDropzone: async p => Assert.Equal("auto", await GetDragCursorAsync(p)));
        });
    }

    [Fact]
    public async Task Should_drop_table_row_with_default_ghost_onto_dropzone()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var defaultGhostRow = page.Locator(".draggable-row", new() { HasTextString = "Alice Anderson" });

            await DragOntoDropzoneAsync(page, defaultGhostRow, dropzoneIndex: 0, whileDragging: async p =>
            {
                // The default drag clone copies the row as-is, so the drag ghost carries the row's name but
                // none of the custom table-row drag ghost's inline table layout styling.
                var ghostHtml = await GetDragGhostHtmlAsync(p);
                Assert.Contains("Alice Anderson", ghostHtml, StringComparison.Ordinal);
                Assert.DoesNotContain("table-layout: fixed", ghostHtml, StringComparison.Ordinal);
            });

            await AssertLastDropAsync(page, "Alice Anderson", "Dropzone 1");
        });
    }

    [Fact]
    public async Task Should_drop_table_row_with_custom_drag_ghost_onto_dropzone()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var customGhostRow = page.Locator(".draggable-row", new() { HasTextString = "Bob Brown" });

            await DragOntoDropzoneAsync(page, customGhostRow, dropzoneIndex: 1, whileDragging: async p =>
            {
                // Bob Brown's row supplies the custom TableRowDragGhost, which rebuilds the cloned row as a
                // self-contained fixed-layout table box. That inline styling is the marker distinguishing it
                // from the plain default row clone.
                var ghostHtml = await GetDragGhostHtmlAsync(p);
                Assert.Contains("Bob Brown", ghostHtml, StringComparison.Ordinal);
                Assert.Contains("table-layout: fixed", ghostHtml, StringComparison.Ordinal);
            });

            await AssertLastDropAsync(page, "Bob Brown", "Dropzone 2");
        });
    }

    [Fact]
    public async Task Should_clear_dropzone_highlight_when_released_without_moving_the_drag_ghost()
    {
        // Arrange
        var browser = new Browser();

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupDraggablePageAsync(page);

            var shapeA = page.Locator(".shape", new() { HasTextString = "Shape A" });

            var shapeABox = await shapeA.BoundingBoxAsync();
            Assert.NotNull(shapeABox);

            var highlightedDropzones = page.Locator(".dropzone.highlighted");

            await page.Mouse.MoveAsync(shapeABox.X + GrabOffset, shapeABox.Y + GrabOffset);
            await page.Mouse.DownAsync();

            // A single move starts the drag and marks the dropzones. Pointer capture then moves to the drag ghost
            // host, which sees no move of its own before the release: a jittery click.
            await page.Mouse.MoveAsync(shapeABox.X + GrabOffset + 1, shapeABox.Y + GrabOffset + 1);

            await Expect(highlightedDropzones.First).ToBeVisibleAsync();

            await page.Mouse.UpAsync();

            await Expect(highlightedDropzones).ToHaveCountAsync(0);
        });
    }

    private async Task SetupDraggablePageAsync(IPage page)
    {
        // Size the viewport large enough that every draggable (including the table rows lower on the page)
        // and both dropzones are visible at once. The drag is driven through viewport pointer coordinates,
        // so the source and target must be on screen together for the whole drag to be hit-tested.
        await page.SetViewportSizeAsync(1280, 1024);

        await page.GotoAsync($"{fixture.ServerAddress}/draggable");

        // Draggables attach their drag interaction after the first render, and the custom table-row ghost
        // additionally imports its own JS module. Wait for the network to go idle so every draggable —
        // including the slower module-backed one — has attached before the drag starts.
        await page.Locator(".shape.draggable").First.WaitForAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task DragOntoDropzoneAsync(
        IPage page,
        ILocator draggable,
        int dropzoneIndex,
        Func<IPage, Task>? whileDragging = null,
        Func<IPage, Task>? overDropzone = null)
    {
        var dropzone = page.Locator(".dropzone").Nth(dropzoneIndex);

        var draggableBox = await draggable.BoundingBoxAsync();
        Assert.NotNull(draggableBox);

        var dropzoneBox = await dropzone.BoundingBoxAsync();
        Assert.NotNull(dropzoneBox);

        // Grab near the draggable's top-left corner rather than its center. The dropzone hit-test uses the
        // drag ghost host's top-left corner, which is positioned as pointer - grabFraction * ghostSize. A wide
        // element (like a table row) grabbed at its center would place that corner far left of the pointer and
        // miss the dropzone, so a near-zero grab fraction keeps the corner tracking the pointer.
        var startX = draggableBox.X + GrabOffset;
        var startY = draggableBox.Y + GrabOffset;
        var endX = dropzoneBox.X + (dropzoneBox.Width / 2);
        var endY = dropzoneBox.Y + (dropzoneBox.Height / 2);

        await page.Mouse.MoveAsync(startX, startY);
        await page.Mouse.DownAsync();

        // The drag only begins on the first pointer move after the press. Nudge first to start the drag,
        // then move to the dropzone in several steps so the dropzone hit-testing runs, and settle on the
        // target before releasing so the drag-enter is registered.
        await page.Mouse.MoveAsync(startX + 5, startY + 5, new() { Steps = 5 });

        // The drag is now in progress and the drag ghost host is on screen. Let the caller inspect the
        // displayed drag ghost before the drop completes and the ghost is discarded.
        if (whileDragging is not null)
            await whileDragging(page);

        await page.Mouse.MoveAsync(endX, endY, new() { Steps = 15 });
        await page.Mouse.MoveAsync(endX, endY);

        if (overDropzone is not null)
            await overDropzone(page);

        await page.Mouse.UpAsync();
    }

    // The drag cursor is the one of the element holding the pointer capture, which is the drag ghost host.
    private static async Task<string> GetDragCursorAsync(IPage page)
        => await page.EvaluateAsync<string>(@"() => {
            const captor = [...document.body.children].find(c => c.hasPointerCapture(1));
            return captor ? getComputedStyle(captor).cursor : '';
        }");

    // The drag ghost applies its cursor once the .NET side has rendered the content for the current state.
    private static async Task WaitForDragCursorAsync(IPage page, string cursor)
        => await page.WaitForFunctionAsync(@"cursor => {
            const captor = [...document.body.children].find(c => c.hasPointerCapture(1));
            return !!captor && getComputedStyle(captor).cursor === cursor;
        }", cursor);

    // Reads the live drag ghost host's inner HTML while a drag is in progress. The host is the absolutely
    // positioned, semi-transparent element the drag interaction appends to <body> to render the drag ghost.
    private static async Task<string> GetDragGhostHtmlAsync(IPage page)
        => await page.EvaluateAsync<string>(@"() => {
            const host = [...document.body.children]
                .find(c => c.style && c.style.position === 'absolute' && c.style.opacity === '0.3');
            return host ? host.innerHTML : '';
        }");

    private static async Task AssertLastDropAsync(IPage page, string expectedLabel, string expectedTarget)
    {
        var summary = page.Locator(".last-drop-summary");

        await Expect(summary).ToContainTextAsync(expectedLabel);
        await Expect(summary).ToContainTextAsync(expectedTarget);
    }
}
