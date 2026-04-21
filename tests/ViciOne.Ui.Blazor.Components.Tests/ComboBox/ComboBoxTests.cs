using System.Collections;
using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.ComboBox;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.ComboBox;

public sealed class ComboBoxTests : IDisposable
{
    private readonly BunitContext _testContext = new();

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
        var a = renderedComponent.Find("option[selected]");
        renderedComponent.Find("option[selected]").InnerHtml.Should().Contain(GetItems().First().Text);
    }

    [Fact]
    public void Should_select_placeholder_if_value_not_matching()
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
        renderedComponent.Find("option[selected][disabled]").Should().NotBeNull();
    }

    [Fact]
    public void Should_select_placeholder_if_no_selection_configured()
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
        renderedComponent.Find("option[selected][disabled]").Should().NotBeNull();
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
            .Add(p => p.Items, updatedItems)
        );

        // Assert
        updatedItems.IterationCount.Should().Be(expectedEnumerations);
    }

    private static List<ComboBoxItem<string, string>> GetItems()
        => [
            new() { Value = "1", Text = "One" },
            new() { Value = "2", Text = "Two" },
            new() { Value = "3", Text = "Three" }
        ];

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
