using Microsoft.Playwright;
using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;

namespace Server.Tests.ComboBox;

[Collection<ServerTestCollection>]
public class ComboBoxTests(ServerFixture fixture)
{
    [Fact]
    public async Task Should_apply_expected_styles_for_active_read_only_and_disabled_states()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/combo-box");

            var comboBoxes = page.Locator(".combo-box");
            Assert.True(await comboBoxes.CountAsync() >= 6, "Expected the ComboBox sample to contain the tested states.");

            var activeComboBox = comboBoxes.Nth(0);
            var readOnlyComboBox = comboBoxes.Nth(4);
            var disabledComboBox = comboBoxes.Nth(5);

            var activeInput = activeComboBox.Locator(".combo-box-input");
            var readOnlyInput = readOnlyComboBox.Locator(".combo-box-input");
            var disabledInput = disabledComboBox.Locator(".combo-box-input");

            // Assert the component and input states
            Assert.False(await HasCssClassAsync(activeComboBox, "disabled"));
            Assert.False(await HasCssClassAsync(activeComboBox, "read-only"));
            Assert.True(await HasCssClassAsync(readOnlyComboBox, "read-only"));
            Assert.True(await HasCssClassAsync(disabledComboBox, "disabled"));

            Assert.False(await activeInput.EvaluateAsync<bool>("element => element.disabled"));
            Assert.True(await readOnlyInput.EvaluateAsync<bool>("element => element.readOnly"));
            Assert.True(await disabledInput.EvaluateAsync<bool>("element => element.disabled"));

            // The inner input remains fully opaque; disabled opacity is applied once by the outer ComboBox.
            Assert.Equal("1", await GetComputedStylePropertyAsync(activeInput, "opacity"));
            Assert.Equal("1", await GetComputedStylePropertyAsync(readOnlyInput, "opacity"));
            Assert.Equal("1", await GetComputedStylePropertyAsync(disabledInput, "opacity"));
            Assert.Equal("1", await GetComputedStylePropertyAsync(activeComboBox, "opacity"));
            Assert.Equal("1", await GetComputedStylePropertyAsync(readOnlyComboBox, "opacity"));
            Assert.Equal("0.5", await GetComputedStylePropertyAsync(disabledComboBox, "opacity"));

            // Hovering the drop-down icon changes the active border only.
            var activeBorderColor = await GetComputedStylePropertyAsync(activeInput, "border-color");
            var activeIconWrapper = activeComboBox.Locator(".drop-down-icon-wrapper");
            await activeIconWrapper.HoverAsync();
            await WaitForAnimationsAsync(activeInput);

            Assert.NotEqual(activeBorderColor, await GetComputedStylePropertyAsync(activeInput, "border-color"));
            Assert.Equal("pointer", await GetComputedStylePropertyAsync(activeIconWrapper, "cursor"));

            await page.Mouse.MoveAsync(0, 0);
            await WaitForAnimationsAsync(activeInput);

            var readOnlyBorderColor = await GetComputedStylePropertyAsync(readOnlyInput, "border-color");
            var readOnlyIconWrapper = readOnlyComboBox.Locator(".drop-down-icon-wrapper");
            Assert.Equal("0.5", await GetComputedStylePropertyAsync(readOnlyIconWrapper, "opacity"));

            await readOnlyIconWrapper.HoverAsync();
            await WaitForAnimationsAsync(readOnlyInput);

            Assert.Equal(readOnlyBorderColor, await GetComputedStylePropertyAsync(readOnlyInput, "border-color"));
            Assert.NotEqual("pointer", await GetComputedStylePropertyAsync(readOnlyIconWrapper, "cursor"));

            await page.Mouse.MoveAsync(0, 0);

            var disabledBorderColor = await GetComputedStylePropertyAsync(disabledInput, "border-color");
            var disabledIconWrapper = disabledComboBox.Locator(".drop-down-icon-wrapper");
            await disabledIconWrapper.HoverAsync();
            await WaitForAnimationsAsync(disabledInput);

            Assert.Equal(disabledBorderColor, await GetComputedStylePropertyAsync(disabledInput, "border-color"));
            Assert.NotEqual("pointer", await GetComputedStylePropertyAsync(disabledIconWrapper, "cursor"));
        });
    }

    [Fact]
    public async Task Should_not_render_drop_down_item_wider_than_combo_box_with_200px_width()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/combo-box");

            // The third ComboBox is the one wrapped in a 200px wide container
            var comboBox = page.Locator(".combo-box").Nth(2);

            // Open the drop-down by clicking the input
            await comboBox.Locator(".combo-box-input").ClickAsync();

            // Assert
            var comboBoxBoundingBox = await comboBox.BoundingBoxAsync();
            Assert.NotNull(comboBoxBoundingBox);

            // The combo-box div is exactly 200px wide
            Assert.True(Math.Abs(comboBoxBoundingBox.Width - 200) < 1,
                $"Expected combo-box width to be 200px, but was {comboBoxBoundingBox.Width}px");

            var dropDownItems = comboBox.Locator(".drop-down-container .drop-down-item");
            var itemCount = await dropDownItems.CountAsync();
            Assert.True(itemCount > 0, "Expected at least one drop-down-item to be rendered");

            var hasOverflowingItem = false;

            for (var i = 0; i < itemCount; i++)
            {
                var item = dropDownItems.Nth(i);

                var itemBoundingBox = await item.BoundingBoxAsync();
                Assert.NotNull(itemBoundingBox);

                // A drop-down-item must never be wider than its parent combo-box
                Assert.True(itemBoundingBox.Width <= comboBoxBoundingBox.Width + 1,
                    $"Expected drop-down-item width ({itemBoundingBox.Width}px) to not exceed combo-box width ({comboBoxBoundingBox.Width}px)");

                // Detect whether the item content is larger than the rendered (clipped) width
                var isOverflowing = await item.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth");

                if (isOverflowing)
                    hasOverflowingItem = true;
            }

            // At least one drop-down-item has content larger than the combo-box (but is still clipped to its width)
            Assert.True(hasOverflowingItem,
                "Expected at least one drop-down-item to have content larger than the combo-box");
        });
    }

    [Fact]
    public async Task Should_scroll_selected_item_to_top_when_drop_down_opens()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/combo-box");

            // The last ComboBox holds 100 items, more than the drop-down shows at once
            var comboBox = page.Locator(".combo-box").Last;
            var input = comboBox.Locator(".combo-box-input");

            await input.FillAsync("Sample Object 50");
            await comboBox.Locator(".drop-down-item", new() { HasText = "Sample Object 50" }).ClickAsync();

            await input.ClickAsync();
            await comboBox.Locator(".drop-down-container.placed").WaitForAsync();

            // Assert
            var containerBoundingBox = await comboBox.Locator(".drop-down-container").BoundingBoxAsync();
            Assert.NotNull(containerBoundingBox);

            var selectedItemBoundingBox = await comboBox.Locator(".drop-down-item.selected").BoundingBoxAsync();
            Assert.NotNull(selectedItemBoundingBox);

            Assert.True(Math.Abs(selectedItemBoundingBox.Y - containerBoundingBox.Y) < 2,
                $"Expected the selected drop-down-item (Y={selectedItemBoundingBox.Y}) to be at the top of the drop-down (Y={containerBoundingBox.Y})");
        });
    }

    [Fact]
    public async Task Should_follow_width_of_container_when_container_gets_narrower_and_wider_again()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/combo-box");

            var container = page.Locator(".resizable-container");
            var comboBox = container.Locator(".combo-box");

            // The drop-down sets its minimum width once after its first render, before that the width is not pinned
            await comboBox.Locator(".drop-down[style*='min-width']").WaitForAsync(new() { State = WaitForSelectorState.Attached });

            var wideWidth = await GetWidthAsync(comboBox);

            await page.GetByRole(AriaRole.Button, new() { Name = "Narrow container" }).ClickAsync();
            await page.Locator(".resizable-container.narrow").WaitForAsync();

            var narrowWidth = await GetWidthAsync(comboBox);
            var narrowContainerWidth = await GetWidthAsync(container);

            await page.GetByRole(AriaRole.Button, new() { Name = "Widen container" }).ClickAsync();
            await page.Locator(".resizable-container:not(.narrow)").WaitForAsync();

            var widenedWidth = await GetWidthAsync(comboBox);

            // Assert
            Assert.True(Math.Abs(wideWidth - 400) < 1,
                $"Expected the ComboBox to fill the 400px container, but was {wideWidth}px");

            Assert.True(Math.Abs(narrowWidth - narrowContainerWidth) < 1,
                $"Expected the ComboBox to shrink to the {narrowContainerWidth}px container, but kept {narrowWidth}px");

            Assert.True(Math.Abs(widenedWidth - 400) < 1,
                $"Expected the ComboBox to fill the 400px container again, but was {widenedWidth}px");
        });
    }

    [Fact]
    public async Task Should_keep_width_when_typing_filters_out_the_widest_items()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/combo-box");

            // The first ComboBox takes the width of its items
            var comboBox = page.Locator(".combo-box").First;
            var input = comboBox.Locator(".combo-box-input");
            var dropDownItems = comboBox.Locator(".drop-down-item");

            await comboBox.Locator(".drop-down[style*='min-width']").WaitForAsync(new() { State = WaitForSelectorState.Attached });

            var initialWidth = await GetWidthAsync(comboBox);
            var initialItemCount = await dropDownItems.CountAsync();

            await input.FillAsync("Sm");
            await comboBox.Locator(".drop-down-container.placed").WaitForAsync();

            var filteredWidth = await GetWidthAsync(comboBox);
            var filteredItemCount = await dropDownItems.CountAsync();

            // Assert
            Assert.True(filteredItemCount < initialItemCount,
                $"Expected typing to filter the items, but {filteredItemCount} of {initialItemCount} items are shown");

            Assert.True(Math.Abs(filteredWidth - initialWidth) < 1,
                $"Expected the ComboBox to keep its width of {initialWidth}px while filtering, but was {filteredWidth}px");
        });
    }

    private static async Task<float> GetWidthAsync(ILocator locator)
    {
        var boundingBox = await locator.BoundingBoxAsync();
        Assert.NotNull(boundingBox);

        return boundingBox.Width;
    }

    private static Task<string> GetComputedStylePropertyAsync(ILocator locator, string propertyName) => locator.EvaluateAsync<string>(
        "(element, propertyName) => getComputedStyle(element).getPropertyValue(propertyName)",
        propertyName);

    private static Task<bool> HasCssClassAsync(ILocator locator, string cssClass) => locator.EvaluateAsync<bool>(
        "(element, cssClass) => element.classList.contains(cssClass)",
        cssClass);

    private static Task<bool> WaitForAnimationsAsync(ILocator locator) => locator.EvaluateAsync<bool>(
        """
        async element => {
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            await Promise.all(element.getAnimations().map(animation => animation.finished));
            return true;
        }
        """);
}
