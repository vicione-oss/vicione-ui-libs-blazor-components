using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;

namespace Server.Tests.Resizeable;

[Collection<ServerTestCollection>]
public class ResizeableTests(ServerFixture fixture)
{
    private const int DragDistance = 25;
    private const int GridSize = 20;
    private const float Tolerance = 2f;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_top_left_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.top-left");

            // Drag up: height should increase, Y should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: -DragDistance, "up");

            // Drag down: height should decrease, Y should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: DragDistance, "down");

            // Drag left: width should increase, X should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: -DragDistance, expectedYChange: 0, "left");

            // Drag right: width should decrease, X should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: DragDistance, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_top_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.top");

            // Drag up: height should increase, Y should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: -DragDistance, "up");

            // Drag down: height should decrease, Y should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: DragDistance, "down");

            // Drag left: no change expected
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "left");

            // Drag right: no change expected
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_top_right_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.top-right");

            // Drag up: height should increase, Y should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: -DragDistance, "up");

            // Drag down: height should decrease, Y should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: DragDistance, "down");

            // Drag left: width should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "left");

            // Drag right: width should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_right_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.right");

            // Drag up: no change expected
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "up");

            // Drag down: no change expected
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "down");

            // Drag left: width should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "left");

            // Drag right: width should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_bottom_right_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.bottom-right");

            // Drag up: height should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: 0, "up");

            // Drag down: height should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: 0, "down");

            // Drag left: width should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "left");

            // Drag right: width should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_bottom_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.bottom");

            // Drag up: height should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: 0, "up");

            // Drag down: height should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: 0, "down");

            // Drag left: no change expected
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "left");

            // Drag right: no change expected
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_bottom_left_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.bottom-left");

            // Drag up: height should decrease
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: -DragDistance, expectedXChange: 0, expectedYChange: 0, "up");

            // Drag down: height should increase
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: DragDistance, expectedXChange: 0, expectedYChange: 0, "down");

            // Drag left: width should increase, X should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: -DragDistance, expectedYChange: 0, "left");

            // Drag right: width should decrease, X should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: DragDistance, expectedYChange: 0, "right");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_resize_correctly_when_dragging_left_handle(bool snapToGrid)
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act & Assert
        await browser.LaunchAsync(async page =>
        {
            await SetupResizeablePageAsync(page, snapToGrid);

            var shape = page.Locator(".shape");
            var handle = shape.Locator(".resize-handle.left");

            // Drag up: no change expected
            await DragAndAssertAsync(page, shape, handle, 0, -DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "up");

            // Drag down: no change expected
            await DragAndAssertAsync(page, shape, handle, 0, DragDistance, snapToGrid,
                expectedWidthChange: 0, expectedHeightChange: 0, expectedXChange: 0, expectedYChange: 0, "down");

            // Drag left: width should increase, X should decrease
            await DragAndAssertAsync(page, shape, handle, -DragDistance, 0, snapToGrid,
                expectedWidthChange: DragDistance, expectedHeightChange: 0, expectedXChange: -DragDistance, expectedYChange: 0, "left");

            // Drag right: width should decrease, X should increase
            await DragAndAssertAsync(page, shape, handle, DragDistance, 0, snapToGrid,
                expectedWidthChange: -DragDistance, expectedHeightChange: 0, expectedXChange: DragDistance, expectedYChange: 0, "right");
        });
    }

    private async Task SetupResizeablePageAsync(IPage page, bool snapToGrid)
    {
        await page.GotoAsync($"{fixture.ServerAddress}/resizeable");

        var gridSizeInput = page.Locator(".spin-edit").GetByRole(AriaRole.Textbox);
        await gridSizeInput.FillAsync(GridSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await gridSizeInput.PressAsync("Enter");

        var resizeableSwitch = page.Locator("button.switch").First;
        await resizeableSwitch.ClickAsync();

        if (snapToGrid)
        {
            var snapToGridSwitch = page.Locator("button.switch").Nth(1);
            await snapToGridSwitch.ClickAsync();
        }

        await page.Locator(".resize-handle").First.WaitForAsync();
    }

    private static async Task DragAndAssertAsync(IPage page, ILocator shape, ILocator handle, int dx, int dy, bool snapToGrid,
        float expectedWidthChange, float expectedHeightChange, float expectedXChange, float expectedYChange, string direction)
    {
        var shapeBefore = await shape.BoundingBoxAsync();
        Assert.NotNull(shapeBefore);

        var handleBox = await handle.BoundingBoxAsync();
        Assert.NotNull(handleBox);

        var handleCenterX = handleBox.X + (handleBox.Width / 2);
        var handleCenterY = handleBox.Y + (handleBox.Height / 2);

        await page.Mouse.MoveAsync(handleCenterX, handleCenterY);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(handleCenterX + dx, handleCenterY + dy);
        await page.Mouse.UpAsync();

        var shapeAfter = await shape.BoundingBoxAsync();
        Assert.NotNull(shapeAfter);

        var actualWidthChange = shapeAfter.Width - shapeBefore.Width;
        var actualHeightChange = shapeAfter.Height - shapeBefore.Height;
        var actualXChange = shapeAfter.X - shapeBefore.X;
        var actualYChange = shapeAfter.Y - shapeBefore.Y;

        if (snapToGrid)
        {
            AssertSnapped(actualWidthChange, expectedWidthChange, "width", direction);
            AssertSnapped(actualHeightChange, expectedHeightChange, "height", direction);
            AssertSnapped(actualXChange, expectedXChange, "X", direction);
            AssertSnapped(actualYChange, expectedYChange, "Y", direction);
        }
        else
        {
            AssertExact(actualWidthChange, expectedWidthChange, "width", direction);
            AssertExact(actualHeightChange, expectedHeightChange, "height", direction);
            AssertExact(actualXChange, expectedXChange, "X", direction);
            AssertExact(actualYChange, expectedYChange, "Y", direction);
        }
    }

    private static void AssertExact(float actual, float expected, string property, string direction)
        => Assert.True(Math.Abs(actual - expected) <= Tolerance,
            $"Dragging {direction}: Expected {property} change of {expected}px, but was {actual}px");

    private static void AssertSnapped(float actual, float expected, string property, string direction)
    {
        if (Math.Abs(expected) < 0.01f)
        {
            Assert.True(Math.Abs(actual) <= Tolerance,
                $"Dragging {direction}: Expected no {property} change, but was {actual}px");
        }
        else
        {
            Assert.True(Math.Sign(actual) == Math.Sign(expected),
                $"Dragging {direction}: Expected {property} change in direction {Math.Sign(expected)}, but was {actual}px");
            Assert.True(Math.Abs(actual % GridSize) <= Tolerance,
                $"Dragging {direction}: Expected {property} change to be a multiple of {GridSize}px, but was {actual}px");
        }
    }
}
