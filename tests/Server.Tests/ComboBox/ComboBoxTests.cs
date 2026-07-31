using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using Xunit;

namespace Server.Tests.ComboBox;

[Collection<ServerTestCollection>]
public class ComboBoxTests(ServerFixture fixture)
{
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
}
