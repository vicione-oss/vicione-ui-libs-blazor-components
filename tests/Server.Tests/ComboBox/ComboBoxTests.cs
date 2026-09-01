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
