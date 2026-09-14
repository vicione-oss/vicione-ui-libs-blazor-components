using System.Collections;
using Bunit;
using ViciOne.Ui.Blazor.Components.ComboBox;

namespace ViciOne.Ui.Blazor.Components.Tests.ComboBox;

public sealed class ComboBoxTests : IDisposable
{
    private readonly BunitContext _testContext;

    public ComboBoxTests()
    {
        _testContext = new BunitContext();

        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
        );

        // Assert
        renderedComponent.Find(".combo-box").Should().NotBeNull();
    }

    [Fact]
    public void Should_render_without_items()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, [])
            .Add(p => p.Value, "")
        );

        // Assert
        renderedComponent.Find(".combo-box").Should().NotBeNull();
    }

    [Fact]
    public void Should_preselect_item_if_set()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, GetItems().First().Value)
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.NoOptionSelected, false)
        );

        // Assert
        renderedComponent.Find(".combo-box input").GetAttribute("value").Should().Be(GetItems().First().Text);
        renderedComponent.Find(".drop-down-item.selected").TextContent.Trim().Should().Be(GetItems().First().Text);
    }

    [Fact]
    public void Should_show_no_selection_if_value_not_matching()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "missing")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.NoOptionSelected, true)
        );

        // Assert
        renderedComponent.Find(".combo-box input").GetAttribute("value").Should().BeNullOrEmpty();
        renderedComponent.FindAll(".drop-down-item.selected").Should().BeEmpty();
    }

    [Fact]
    public void Should_show_no_selection_if_no_selection_configured()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "1")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.NoOptionSelected, true)
        );

        // Assert
        renderedComponent.Find(".combo-box input").GetAttribute("value").Should().BeNullOrEmpty();
        renderedComponent.FindAll(".drop-down-item.selected").Should().BeEmpty();
    }

    [Fact]
    public void Should_raise_value_changed_on_drop_down_item_click()
    {
        // Arrange
        string? changedValue = null;

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.ValueChanged, v => changedValue = v)
        );

        // Act
        renderedComponent.FindAll(".drop-down-item")[1].Click();

        // Assert
        changedValue.Should().Be(GetItems()[1].Value);
    }

    [Fact]
    public void Should_render_editable_input_when_allow_user_input()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.AllowUserInput, true)
        );

        // Assert
        renderedComponent.Find(".combo-box input").HasAttribute("readonly").Should().BeFalse();
    }

    [Fact]
    public void Should_filter_drop_down()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
        );

        // Act
        renderedComponent.Find(".combo-box input").Input("o");

        // Assert - only "One" and "Two" contain 'o'
        renderedComponent.FindAll(".drop-down-item").Should().HaveCount(2);
    }

    [Fact]
    public void Should_show_empty_state_when_input_matches_no_item()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
        );

        // Act
        renderedComponent.Find(".combo-box input").Input("no-such-item");

        // Assert
        renderedComponent.FindAll(".drop-down-item:not(.drop-down-empty)").Should().BeEmpty();

        renderedComponent.Find(".drop-down-empty").Should().NotBeNull();
    }

    [Fact]
    public void Should_filter_drop_down_without_user_input()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        // Act
        renderedComponent.Find(".combo-box input").Input("o");

        // Assert - input is editable for filtering and only "One"/"Two" contain 'o'
        renderedComponent.Find(".combo-box input").HasAttribute("readonly").Should().BeFalse();
        renderedComponent.FindAll(".drop-down-item").Should().HaveCount(2);
    }

    [Fact]
    public void Should_commit_value_on_enter_when_allow_user_input()
    {
        // Arrange
        string? changedValue = null;

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
            .Add(p => p.ValueChanged, v => changedValue = v)
        );

        // Act
        var input = renderedComponent.Find(".combo-box input");
        input.Click();
        input.Input("custom");
        input.KeyUp(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        changedValue.Should().Be("custom");
    }

    [Fact]
    public void Should_open_drop_down_on_enter()
    {
        // Arrange
        string? changedValue = null;

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.ValueChanged, v => changedValue = v)
        );

        // Act
        var input = renderedComponent.Find(".combo-box input");
        input.KeyUp(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        renderedComponent.Find(".drop-down-container").ClassList.Should().Contain("visible");
    }

    [Fact]
    public void Should_open_drop_down_on_input()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        // Act
        renderedComponent.Find(".combo-box input").Input("o");

        // Assert
        renderedComponent.Find(".drop-down-container").ClassList.Should().Contain("visible");
    }

    [Fact]
    public void Should_commit_custom_value_on_blur_when_allow_user_input()
    {
        // Arrange
        string? changedValue = null;

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
            .Add(p => p.ValueChanged, v => changedValue = v)
        );

        // Act
        var input = renderedComponent.Find(".combo-box input");
        input.Input("custom");
        input.Blur(new Microsoft.AspNetCore.Components.Web.FocusEventArgs());

        // Assert
        changedValue.Should().Be("custom");
    }

    [Fact]
    public void Should_ignore_custom_value_that_cannot_convert_to_value_type()
    {
        // Arrange
        var changed = false;

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<int, string>, int>>(builder => builder
            .Add(p => p.Items, GetNumericItems())
            .Add(p => p.Value, 0)
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
            .Add(p => p.ValueChanged, _ => changed = true)
        );

        // Act
        var input = renderedComponent.Find(".combo-box input");
        input.Input("not-a-number");
        input.Blur(new Microsoft.AspNetCore.Components.Web.FocusEventArgs());

        // Assert
        changed.Should().BeFalse();
    }

    [Fact]
    public void Should_commit_matching_item_on_blur_for_non_convertible_value_type()
    {
        // Arrange
        SampleValue? changedValue = null;
        var items = GetSampleValueItems();

        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<SampleValue, string>, SampleValue>>(builder => builder
            .Add(p => p.Items, items)
            .Add(p => p.Value, items[0].Value)
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
            .Add(p => p.AllowUserInput, true)
            .Add(p => p.ValueChanged, v => changedValue = v)
        );

        // Act
        var input = renderedComponent.Find(".combo-box input");
        input.Input("Large");
        input.Blur(new Microsoft.AspNetCore.Components.Web.FocusEventArgs());

        // Assert
        changedValue.Should().Be(items[1].Value);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void Should_avoid_one_iteration_when_collection_and_item_count_changed(bool isCollection, int expectedEnumerations)
    {
        // Arrange
        var updatedItems = isCollection
                ? new TestCollection<ComboBoxItem<string, string>>([.. GetItems().Skip(1)])
                : new TestEnumerable<ComboBoxItem<string, string>>([.. GetItems().Skip(1)]);

        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, GetItems().First().Value)
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        renderedComponent.Render(builder => builder
            .Add(p => p.Items, updatedItems));

        // Assert
        updatedItems.IterationCount.Should().Be(expectedEnumerations);
    }

    [Fact]
    public void Should_render_title_on_drop_down_items()
    {
        // Act
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        // Assert - each item exposes its text as the title (tooltip)
        var items = renderedComponent.FindAll(".drop-down-item");
        items.Select(item => item.GetAttribute("title"))
            .Should().Equal(GetItems().Select(item => item.Text));
    }

    [Fact]
    public void Should_open_drop_down_on_icon_click()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        // Act
        renderedComponent.Find(".drop-down-icon-wrapper").Click();

        // Assert - the menu is shown and the icon reflects the open state
        renderedComponent.Find(".drop-down-container").ClassList.Should().Contain("visible");
        renderedComponent.FindAll(".monochrome-icon-expander-light-top").Should().ContainSingle();
    }

    [Fact]
    public void Should_toggle_drop_down_on_icon_click()
    {
        // Arrange
        var renderedComponent = _testContext.Render<ComboBox<ComboBoxItem<string, string>, string>>(builder => builder
            .Add(p => p.Items, GetItems())
            .Add(p => p.Value, "")
            .Add(p => p.ValueSelector, x => x.Value)
            .Add(p => p.TextSelector, x => x.Text)
        );

        var iconWrapper = renderedComponent.Find(".drop-down-icon-wrapper");

        // Act - open
        iconWrapper.Click();

        // Assert - open
        renderedComponent.Find(".drop-down-container").ClassList.Should().Contain("visible");

        // Act - close
        renderedComponent.Find(".drop-down-icon-wrapper").Click();

        // Assert - closed and the icon points down again
        renderedComponent.Find(".drop-down-container").ClassList.Should().NotContain("visible");
        renderedComponent.FindAll(".monochrome-icon-expander-light-down").Should().ContainSingle();
    }

    private static List<ComboBoxItem<string, string>> GetItems()
        => [
            new() { Value = "1", Text = "One" },
            new() { Value = "2", Text = "Two" },
            new() { Value = "3", Text = "Three" }
        ];

    private static List<ComboBoxItem<int, string>> GetNumericItems()
        => [
            new() { Value = 1, Text = "1" },
            new() { Value = 2, Text = "2" },
            new() { Value = 3, Text = "3" }
        ];

    private static List<ComboBoxItem<SampleValue, string>> GetSampleValueItems()
        => [
            new() { Value = new SampleValue("Small"), Text = "Small" },
            new() { Value = new SampleValue("Large"), Text = "Large" }
        ];

    public sealed record SampleValue(string Name);

    public class TestEnumerable<T> : IEnumerable<T>
    {
        protected IEnumerable<T> Items { get; set; } = [];

        public int IterationCount { get; private set; }

        public TestEnumerable() { }

        public TestEnumerable(IEnumerable<T> items)
            => Items = items;

        public IEnumerator<T> GetEnumerator()
        {
            IterationCount++;
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TestCollection<T> : TestEnumerable<T>, ICollection<T>
    {
        public int Count => Items.Count();
        public bool IsReadOnly => true;

        public TestCollection(IEnumerable<T> items)
            => Items = items;

        public void Add(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(T item) => throw new NotSupportedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
