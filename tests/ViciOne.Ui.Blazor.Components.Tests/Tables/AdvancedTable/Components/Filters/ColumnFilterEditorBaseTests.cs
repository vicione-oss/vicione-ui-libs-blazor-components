using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Filters;

// Driven through a two-value editor throughout: the base must host a lower/upper bound pair as readily as a
// single string. The one-value path stays covered by SimpleTableContainsColumnFilterEditorTests.
public sealed class ColumnFilterEditorBaseTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public ColumnFilterEditorBaseTests()
        => _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Editor_placed_outside_a_filter_slot_throws_naming_the_slot()
    {
        // Act — no cascading context, which is what a placement outside a column's filter slot looks like
        var render = () => _testContext.Render<TestNumericRangeColumnFilterEditor>();

        // Assert
        render.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{nameof(TestNumericRangeColumnFilterEditor)}*FilterEditor*");
    }

    [Fact]
    public void Apply_commits_the_built_filter_and_then_closes()
    {
        // Arrange
        var events = new List<string>();
        IColumnFilter? appliedFilter = null;
        var rendered = RenderEditor(
            EventCallback.Factory.Create<IColumnFilter?>(this, filter =>
            {
                appliedFilter = filter;
                events.Add("changed");
            }),
            EventCallback.Factory.Create(this, () => events.Add("closed")));

        // Act — both bounds, so the draft the base knows nothing about is what reaches the filter
        rendered.Find(".range-from").Change("5");
        rendered.Find(".range-to").Change("10");
        rendered.Find(".apply").Click();

        // Assert
        appliedFilter.Should().BeOfType<TestNumericRangeColumnFilter>();
        appliedFilter.As<TestNumericRangeColumnFilter>().ColumnId.Should().Be("TestColumn");
        appliedFilter.As<TestNumericRangeColumnFilter>().From.Should().Be(5);
        appliedFilter.As<TestNumericRangeColumnFilter>().To.Should().Be(10);

        events.Should().Equal(["changed", "closed"]);
    }

    [Fact]
    public void Apply_with_an_empty_draft_clears_the_column_filter_and_still_closes()
    {
        // Arrange
        var raisedFilterChange = false;
        IColumnFilter? appliedFilter = new TestNumericRangeColumnFilter();
        var closeRequestCount = 0;
        var rendered = RenderEditor(
            EventCallback.Factory.Create<IColumnFilter?>(this, filter =>
            {
                appliedFilter = filter;
                raisedFilterChange = true;
            }),
            EventCallback.Factory.Create(this, () => closeRequestCount++));

        // Act — neither bound entered, which is what this editor treats as empty
        rendered.Find(".apply").Click();

        // Assert
        raisedFilterChange.Should().BeTrue();
        appliedFilter.Should().BeNull();
        closeRequestCount.Should().Be(1);
    }

    [Fact]
    public void Cancel_closes_without_committing_a_filter()
    {
        // Arrange
        var raisedFilterChange = false;
        var closeRequestCount = 0;
        var rendered = RenderEditor(
            EventCallback.Factory.Create<IColumnFilter?>(this, _ => raisedFilterChange = true),
            EventCallback.Factory.Create(this, () => closeRequestCount++));

        // Act
        rendered.Find(".range-from").Change("5");
        rendered.Find(".cancel").Click();

        // Assert
        raisedFilterChange.Should().BeFalse();
        closeRequestCount.Should().Be(1);
    }

    [Fact]
    public void Draft_seeds_from_the_columns_current_filter()
    {
        // Act
        var rendered = RenderEditor(currentFilter: new TestNumericRangeColumnFilter("TestColumn", 3, 7));

        // Assert — both bounds seeded, neither of them privileged by the base
        rendered.Find(".range-from").GetAttribute("value").Should().Be("3");
        rendered.Find(".range-to").GetAttribute("value").Should().Be("7");
    }

    [Fact]
    public void Draft_seeds_empty_for_a_filter_kind_the_editor_does_not_recognize()
    {
        // Act — a filter on the same column, of a type this editor cannot read
        var rendered = RenderEditor(currentFilter: new TestColumnFilter());

        // Assert
        rendered.Find(".range-from").HasAttribute("value").Should().BeFalse();
        rendered.Find(".range-to").HasAttribute("value").Should().BeFalse();
    }

    [Fact]
    public void Seeded_draft_is_committed_unchanged_when_apply_is_clicked_without_an_edit()
    {
        // Arrange
        IColumnFilter? appliedFilter = null;
        var rendered = RenderEditor(
            EventCallback.Factory.Create<IColumnFilter?>(this, filter => appliedFilter = filter),
            currentFilter: new TestNumericRangeColumnFilter("TestColumn", 3, null));

        // Act
        rendered.Find(".apply").Click();

        // Assert — an upper bound left unbounded stays unbounded rather than collapsing the draft to empty
        appliedFilter.Should().BeOfType<TestNumericRangeColumnFilter>();
        appliedFilter.As<TestNumericRangeColumnFilter>().From.Should().Be(3);
        appliedFilter.As<TestNumericRangeColumnFilter>().To.Should().BeNull();
    }

    private IRenderedComponent<TestNumericRangeColumnFilterEditor> RenderEditor(
        EventCallback<IColumnFilter?>? filterChanged = null, EventCallback? closeRequested = null,
        IColumnFilter? currentFilter = null)
            => _testContext.Render<TestNumericRangeColumnFilterEditor>(b => b
                .AddCascadingValue(
                    new ColumnFilterEditorContext("TestColumn", currentFilter,
                        filterChanged ?? EventCallback<IColumnFilter?>.Empty,
                        closeRequested ?? EventCallback.Empty)));
}
