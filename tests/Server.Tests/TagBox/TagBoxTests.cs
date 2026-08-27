using Server.Tests.Infrastructure;
using ViciOne.Ui.Testing.Playwright.Infrastructure;
using Xunit;

namespace Server.Tests.TagBox;

[Collection<ServerTestCollection>]
public class TagBoxTests(ServerFixture fixture)
{
    [Fact]
    public async Task Should_expand_width_without_line_break_when_typing_long_text()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            var firstTagBox = page.Locator(".tag-box").First;
            var tagInput = firstTagBox.Locator(".tag-input");

            // Focus the first TagBox by clicking its input field
            await tagInput.ClickAsync();

            // Remember the width of the TagBox
            var initialBoundingBox = await firstTagBox.BoundingBoxAsync();
            Assert.NotNull(initialBoundingBox);
            var initialWidth = initialBoundingBox.Width;
            var initialHeight = initialBoundingBox.Height;

            // Enter long text
            await tagInput.PressSequentiallyAsync("em ipsum dolor sit amet, consetetur sadipscing elitr, sed diam");

            // Assert
            var newBoundingBox = await firstTagBox.BoundingBoxAsync();
            Assert.NotNull(newBoundingBox);

            // Verify that the tagbox got wider
            Assert.True(newBoundingBox.Width > initialWidth,
                $"Expected tagbox to be wider than {initialWidth}px, but was {newBoundingBox.Width}px");

            // Verify it's not a 2 liner (height should remain the same)
            Assert.True(newBoundingBox.Height <= initialHeight + 1,
                $"Expected tagbox height to remain approximately {initialHeight}px (not a 2 liner), but was {newBoundingBox.Height}px");
        });
    }

    [Fact]
    public async Task Should_wrap_tags_into_three_lines_without_expanding_width()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            var thirdTagBox = page.Locator(".tag-box").Nth(2);
            var tagInput = thirdTagBox.Locator(".tag-input");

            // Focus the third TagBox by clicking its input field
            await tagInput.ClickAsync();

            // Remember the width of the TagBox
            var initialBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(initialBoundingBox);
            var initialWidth = initialBoundingBox.Width;

            // Click all tags in the drop-down menu to add them to the TagBox
            var dropDown = thirdTagBox.Locator(".drop-down-container");
            var availableTagCount = await dropDown.Locator(".drop-down-item").CountAsync();

            for (var i = 0; i < availableTagCount; i++)
            {
                // Always click the first non-selected tag since the list re-renders after each click
                var nextTag = dropDown.Locator(".drop-down-item:not(.selected)").First;

                if (await nextTag.CountAsync() == 0)
                    break;

                await nextTag.ClickAsync();
            }

            // Assert
            var finalBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(finalBoundingBox);

            // Verify the TagBox didn't get wider
            Assert.True(finalBoundingBox.Width <= initialWidth + 1,
                $"Expected tagbox width to remain approximately {initialWidth}px, but was {finalBoundingBox.Width}px");

            // Verify the TagBox is a 3 liner by checking the content area height
            var content = thirdTagBox.Locator(".content");
            var tagElements = content.Locator(".tag-element");
            var tagCount = await tagElements.CountAsync();
            Assert.Equal(5, tagCount);

            // Get the height of a single tag to determine expected line height
            var firstTagBox2 = await tagElements.First.BoundingBoxAsync();
            Assert.NotNull(firstTagBox2);
            var singleLineHeight = firstTagBox2.Height;

            // The content should span 3 lines (height should be approximately 3x a single tag height)
            var contentBoundingBox = await content.BoundingBoxAsync();
            Assert.NotNull(contentBoundingBox);

            var expectedMinHeightFor3Lines = singleLineHeight * 2.5;
            var expectedMaxHeightFor4Lines = singleLineHeight * 4;
            Assert.True(contentBoundingBox.Height >= expectedMinHeightFor3Lines,
                $"Expected content height >= {expectedMinHeightFor3Lines}px (3 lines), but was {contentBoundingBox.Height}px");
            Assert.True(contentBoundingBox.Height < expectedMaxHeightFor4Lines,
                $"Expected content height < {expectedMaxHeightFor4Lines}px (less than 4 lines), but was {contentBoundingBox.Height}px");

            // Verify the input field is behind a tag (on the same line), not on a separate line
            var lastTag = tagElements.Last;
            var lastTagBox = await lastTag.BoundingBoxAsync();
            Assert.NotNull(lastTagBox);

            var inputBox = await tagInput.BoundingBoxAsync();
            Assert.NotNull(inputBox);

            // The input should be on the same vertical line as the last tag (same Y position)
            Assert.True(Math.Abs(inputBox.Y - lastTagBox.Y) < singleLineHeight,
                $"Expected input (Y={inputBox.Y}) to be on the same line as last tag (Y={lastTagBox.Y})");
        });
    }

    [Fact]
    public async Task Should_place_input_on_separate_line_when_typing_long_text()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            var thirdTagBox = page.Locator(".tag-box").Nth(2);
            var tagInput = thirdTagBox.Locator(".tag-input");

            // Focus the third TagBox by clicking its input field
            await tagInput.ClickAsync();

            // Remember the width of the TagBox
            var initialBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(initialBoundingBox);
            var initialWidth = initialBoundingBox.Width;

            // Click all tags in the drop-down menu to add them to the TagBox
            var dropDown = thirdTagBox.Locator(".drop-down-container");
            var availableTagCount = await dropDown.Locator(".drop-down-item").CountAsync();

            for (var i = 0; i < availableTagCount; i++)
            {
                var nextTag = dropDown.Locator(".drop-down-item:not(.selected)").First;

                if (await nextTag.CountAsync() == 0)
                    break;

                await nextTag.ClickAsync();
            }

            // Type long text without pressing Enter
            await tagInput.PressSequentiallyAsync("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam");

            // Assert
            var finalBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(finalBoundingBox);

            // Verify the TagBox didn't get wider
            Assert.True(finalBoundingBox.Width <= initialWidth + 1,
                $"Expected tagbox width to remain approximately {initialWidth}px, but was {finalBoundingBox.Width}px");

            // Verify the TagBox is a 4 liner
            var content = thirdTagBox.Locator(".content");
            var tagElements = content.Locator(".tag-element");

            var firstTagBoundingBox = await tagElements.First.BoundingBoxAsync();
            Assert.NotNull(firstTagBoundingBox);
            var singleLineHeight = firstTagBoundingBox.Height;

            var contentBoundingBox = await content.BoundingBoxAsync();
            Assert.NotNull(contentBoundingBox);

            var expectedMinHeightFor4Lines = singleLineHeight * 3.5;
            var expectedMaxHeightFor5Lines = singleLineHeight * 5;
            Assert.True(contentBoundingBox.Height >= expectedMinHeightFor4Lines,
                $"Expected content height >= {expectedMinHeightFor4Lines}px (4 lines), but was {contentBoundingBox.Height}px");
            Assert.True(contentBoundingBox.Height < expectedMaxHeightFor5Lines,
                $"Expected content height < {expectedMaxHeightFor5Lines}px (less than 5 lines), but was {contentBoundingBox.Height}px");

            // Verify the input field is on a separate line, not behind a tag
            var lastTag = tagElements.Last;
            var lastTagBoundingBox = await lastTag.BoundingBoxAsync();
            Assert.NotNull(lastTagBoundingBox);

            var inputBox = await tagInput.BoundingBoxAsync();
            Assert.NotNull(inputBox);

            Assert.True(inputBox.Y > lastTagBoundingBox.Y + (lastTagBoundingBox.Height / 2),
                $"Expected input (Y={inputBox.Y}) to be on a separate line below last tag (Y={lastTagBoundingBox.Y}, height={lastTagBoundingBox.Height})");
        });
    }

    [Fact]
    public async Task Should_collapse_to_two_lines_after_removing_tags()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            var thirdTagBox = page.Locator(".tag-box").Nth(2);
            var tagInput = thirdTagBox.Locator(".tag-input");

            // Focus the third TagBox by clicking its input field
            await tagInput.ClickAsync();

            // Remember the width of the TagBox
            var initialBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(initialBoundingBox);
            var initialWidth = initialBoundingBox.Width;

            // Click all tags in the drop-down menu to add them to the TagBox
            var dropDown = thirdTagBox.Locator(".drop-down-container");
            var availableTagCount = await dropDown.Locator(".drop-down-item").CountAsync();

            for (var i = 0; i < availableTagCount; i++)
            {
                var nextTag = dropDown.Locator(".drop-down-item:not(.selected)").First;

                if (await nextTag.CountAsync() == 0)
                    break;

                await nextTag.ClickAsync();
            }

            // Add a new custom tag and press Enter
            await tagInput.PressSequentiallyAsync("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam");
            await tagInput.PressAsync("Enter");

            // Remove the newly added custom tag by pressing the delete button
            var content = thirdTagBox.Locator(".content");
            const string CustomTagText = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam";
            var customTag = content.Locator(".tag-element").Filter(new() { HasText = CustomTagText });
            var customTagDeleteButton = customTag.Locator(".delete-button");
            await customTagDeleteButton.ClickAsync();

            // Remove the "Cluster" tag by pressing the delete button
            var clusterTag = content.Locator(".tag-element").Filter(new() { HasText = "Cluster" });
            var clusterTagDeleteButton = clusterTag.Locator(".delete-button");
            await clusterTagDeleteButton.ClickAsync();

            // Assert
            var finalBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(finalBoundingBox);

            // Verify the TagBox didn't get wider
            Assert.True(finalBoundingBox.Width <= initialWidth + 1,
                $"Expected tagbox width to remain approximately {initialWidth}px, but was {finalBoundingBox.Width}px");

            // Verify the custom tag and Cluster tag don't exist
            var tagElements = content.Locator(".tag-element");
            var tagCount = await tagElements.CountAsync();
            Assert.Equal(4, tagCount);

            var allTagTexts = await tagElements.AllTextContentsAsync();
            Assert.DoesNotContain("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam", allTagTexts);
            Assert.DoesNotContain("Cluster", allTagTexts);

            // Verify the TagBox is a 2 liner
            var firstTagBoundingBox = await tagElements.First.BoundingBoxAsync();
            Assert.NotNull(firstTagBoundingBox);
            var singleLineHeight = firstTagBoundingBox.Height;

            var contentBoundingBox = await content.BoundingBoxAsync();
            Assert.NotNull(contentBoundingBox);

            var expectedMinHeightFor2Lines = singleLineHeight * 1.5;
            var expectedMaxHeightFor3Lines = singleLineHeight * 3;
            Assert.True(contentBoundingBox.Height >= expectedMinHeightFor2Lines,
                $"Expected content height >= {expectedMinHeightFor2Lines}px (2 lines), but was {contentBoundingBox.Height}px");
            Assert.True(contentBoundingBox.Height < expectedMaxHeightFor3Lines,
                $"Expected content height < {expectedMaxHeightFor3Lines}px (less than 3 lines), but was {contentBoundingBox.Height}px");

            // Verify the input field is on the last line behind a tag
            var lastTag = tagElements.Last;
            var lastTagBoundingBox = await lastTag.BoundingBoxAsync();
            Assert.NotNull(lastTagBoundingBox);

            var inputBox = await tagInput.BoundingBoxAsync();
            Assert.NotNull(inputBox);

            Assert.True(Math.Abs(inputBox.Y - lastTagBoundingBox.Y) < singleLineHeight,
                $"Expected input (Y={inputBox.Y}) to be on the same line as last tag (Y={lastTagBoundingBox.Y})");
        });
    }

    [Fact]
    public async Task Should_collapse_to_three_lines_after_shortening_input()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            var thirdTagBox = page.Locator(".tag-box").Nth(2);
            var tagInput = thirdTagBox.Locator(".tag-input");

            // Focus the third TagBox by clicking its input field
            await tagInput.ClickAsync();

            // Remember the width of the TagBox
            var initialBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(initialBoundingBox);
            var initialWidth = initialBoundingBox.Width;

            // Click all tags in the drop-down menu to add them to the TagBox
            var dropDown = thirdTagBox.Locator(".drop-down-container");
            var availableTagCount = await dropDown.Locator(".drop-down-item").CountAsync();

            for (var i = 0; i < availableTagCount; i++)
            {
                var nextTag = dropDown.Locator(".drop-down-item:not(.selected)").First;

                if (await nextTag.CountAsync() == 0)
                    break;

                await nextTag.ClickAsync();
            }

            // Type long text without pressing Enter
            const string LongText = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam";
            await tagInput.PressSequentiallyAsync(LongText);

            // Press backspace to shorten the text to "Lorem ipsum dolor"
            const string TargetText = "Lorem ipsum dolor";
            var charsToDelete = LongText.Length - TargetText.Length;

            for (var i = 0; i < charsToDelete; i++)
                await tagInput.PressAsync("Backspace");

            // Assert
            var finalBoundingBox = await thirdTagBox.BoundingBoxAsync();
            Assert.NotNull(finalBoundingBox);

            // Verify the TagBox didn't get wider
            Assert.True(finalBoundingBox.Width <= initialWidth + 1,
                $"Expected tagbox width to remain approximately {initialWidth}px, but was {finalBoundingBox.Width}px");

            // Verify the TagBox is a 3 liner
            var content = thirdTagBox.Locator(".content");
            var tagElements = content.Locator(".tag-element");

            var firstTagBoundingBox = await tagElements.First.BoundingBoxAsync();
            Assert.NotNull(firstTagBoundingBox);
            var singleLineHeight = firstTagBoundingBox.Height;

            var contentBoundingBox = await content.BoundingBoxAsync();
            Assert.NotNull(contentBoundingBox);

            var expectedMinHeightFor3Lines = singleLineHeight * 2.5;
            var expectedMaxHeightFor4Lines = singleLineHeight * 4;
            Assert.True(contentBoundingBox.Height >= expectedMinHeightFor3Lines,
                $"Expected content height >= {expectedMinHeightFor3Lines}px (3 lines), but was {contentBoundingBox.Height}px");
            Assert.True(contentBoundingBox.Height < expectedMaxHeightFor4Lines,
                $"Expected content height < {expectedMaxHeightFor4Lines}px (less than 4 lines), but was {contentBoundingBox.Height}px");

            // Verify the input field is on the last line behind a tag
            var lastTag = tagElements.Last;
            var lastTagBoundingBox = await lastTag.BoundingBoxAsync();
            Assert.NotNull(lastTagBoundingBox);

            var inputBox = await tagInput.BoundingBoxAsync();
            Assert.NotNull(inputBox);

            Assert.True(Math.Abs(inputBox.Y - lastTagBoundingBox.Y) < singleLineHeight,
                $"Expected input (Y={inputBox.Y}) to be on the same line as last tag (Y={lastTagBoundingBox.Y})");
        });
    }

    [Fact]
    public async Task Should_not_render_drop_down_item_wider_than_tag_box_with_200px_width()
    {
        // Arrange
        var browser = new Browser()
            .WithOptions(new() { SlowMo = 200 });

        // Act
        await browser.LaunchAsync(async page =>
        {
            await page.GotoAsync($"{fixture.ServerAddress}/tag-box");

            // The dedicated 200px-width TagBox contains an item wider than the box itself
            var tagBox = page.Locator(".tag-box").Nth(3);

            // Open the drop-down by clicking the input
            await tagBox.Locator(".tag-input").ClickAsync();

            // Assert
            var tagBoxBoundingBox = await tagBox.BoundingBoxAsync();
            Assert.NotNull(tagBoxBoundingBox);

            // The tag-box div is exactly 200px wide
            Assert.True(Math.Abs(tagBoxBoundingBox.Width - 200) < 1,
                $"Expected tag-box width to be 200px, but was {tagBoxBoundingBox.Width}px");

            var dropDownItems = tagBox.Locator(".drop-down-container .drop-down-item");
            var itemCount = await dropDownItems.CountAsync();
            Assert.True(itemCount > 0, "Expected at least one drop-down-item to be rendered");

            var hasOverflowingItem = false;

            for (var i = 0; i < itemCount; i++)
            {
                var item = dropDownItems.Nth(i);

                var itemBoundingBox = await item.BoundingBoxAsync();
                Assert.NotNull(itemBoundingBox);

                // A drop-down-item must never be wider than its parent tag-box
                Assert.True(itemBoundingBox.Width <= tagBoxBoundingBox.Width + 1,
                    $"Expected drop-down-item width ({itemBoundingBox.Width}px) to not exceed tag-box width ({tagBoxBoundingBox.Width}px)");

                // Detect whether the item content is larger than the rendered (clipped) width
                var isOverflowing = await item.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth");

                if (isOverflowing)
                    hasOverflowingItem = true;
            }

            // At least one drop-down-item has content larger than the tag-box (but is still clipped to its width)
            Assert.True(hasOverflowingItem,
                "Expected at least one drop-down-item to have content larger than the tag-box");
        });
    }
}
