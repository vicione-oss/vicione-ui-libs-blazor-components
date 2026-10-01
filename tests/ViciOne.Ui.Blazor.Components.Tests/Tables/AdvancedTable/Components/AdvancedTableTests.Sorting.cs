using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

// State-ownership tests for SortingState — the twins of the FilterState ones. The rules are shared, but the sorting
// wiring is separate, so these guard it against a regression that would otherwise pass every other sorting
// test.
public sealed partial class AdvancedTableTests
{
    [Fact]
    public async Task Setting_the_sorting_state_applies_and_refetches_once()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .AddSortableColumn("col"));

        var callsAfterInit = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(
            SortingState.Empty.WithColumnSorting("col", ascending: true)));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit + 1);
        rendered.Find("th").GetAttribute("aria-sort").Should().Be("ascending");
    }

    [Fact]
    public async Task Setting_the_applied_sorting_state_again_neither_raises_nor_refetches()
    {
        // Arrange
        var sorting = SortingState.Empty.WithColumnSorting("col", ascending: true);
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, sorting)
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, _ => raised++))
            .AddSortableColumn("col"));

        var callsAfterInit = provider.CallCount;
        var raisedAfterInit = raised;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(sorting));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit);
        raised.Should().Be(raisedAfterInit);
    }

    [Fact]
    public void Sorting_parameter_instance_change_applies_and_raises()
    {
        // Arrange — a new instance is a command, honored for the whole lifetime of the table
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged,
                EventCallback.Factory.Create<SortingState>(this, sortingState => raised = sortingState))
            .AddSortableColumn("col"));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Render(b => b
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("col", ascending: true)));

        // Assert
        rendered.WaitForAssertion(() =>
        {
            provider.CallCount.Should().Be(callsAfterInit + 1);
            rendered.Find("th").GetAttribute("aria-sort").Should().Be("ascending");

            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().ContainSingle();
        });
    }

    [Fact]
    public void Unchanged_sorting_parameter_instance_is_ignored_after_the_table_moved_its_own_state()
    {
        // Arrange — the starting-value pattern: one instance, handed over on every render and never replaced
        var seed = SortingState.Empty.WithColumnSorting("col", ascending: true);
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, seed)
            .AddSortableColumn("col"));

        // The table moves its own state, or the re-render below would prove nothing
        rendered.Find(".sortable").Click();
        rendered.WaitForAssertion(() => rendered.Find("th").GetAttribute("aria-sort").Should().Be("descending"));

        var callsAfterHeaderClick = provider.CallCount;

        // Act — an unrelated re-render hands the same instance over again
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the seed does not put the table back where it started
        provider.CallCount.Should().Be(callsAfterHeaderClick);
        rendered.Find("th").GetAttribute("aria-sort").Should().Be("descending");
    }

    [Fact]
    public void Own_sorting_value_fed_back_as_parameter_neither_raises_nor_refetches()
    {
        // Arrange — what a correctly bound parent does on every render; the round trip has to terminate
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;
        SortingState? lastRaised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sortingState =>
            {
                raised++;
                lastRaised = sortingState;
            }))
            .AddSortableColumn("col"));

        rendered.Find(".sortable").Click();
        rendered.WaitForAssertion(() => lastRaised.Should().NotBeNull());

        var callsAfterHeaderClick = provider.CallCount;
        var raisedAfterHeaderClick = raised;

        // Act
        rendered.Render(b => b
            .Add(p => p.SortingState, lastRaised));

        // Assert
        provider.CallCount.Should().Be(callsAfterHeaderClick);
        raised.Should().Be(raisedAfterHeaderClick);
    }

    [Fact]
    public void Sorting_parameter_ignored_by_a_guard_is_still_recorded_as_last_received()
    {
        // Arrange — a parent that binds but stops handling the event. Its value is rejected as the table's own
        // on the way in; unless that instance is recorded anyway, the very same value reads as a fresh command
        // once the table has moved on, and silently reverts the user.
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? lastRaised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged,
                EventCallback.Factory.Create<SortingState>(this, sortingState => lastRaised = sortingState))
            .AddSortableColumn("col"));

        rendered.Find(".sortable").Click();
        rendered.WaitForAssertion(() => rendered.Find("th").GetAttribute("aria-sort").Should().Be("ascending"));

        // The parent hands the table's own value back — rejected by the own-value guard, recorded regardless
        var boundValue = lastRaised;
        boundValue.Should().NotBeNull();
        rendered.Render(b => b
            .Add(p => p.SortingState, boundValue));

        // The user reverses the sort and the parent does not follow along
        rendered.Find(".sortable").Click();
        rendered.WaitForAssertion(() => rendered.Find("th").GetAttribute("aria-sort").Should().Be("descending"));

        var callsAfterSecondClick = provider.CallCount;

        // Act — an unrelated re-render, still carrying the parent's stale value
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the reversed sort survives
        provider.CallCount.Should().Be(callsAfterSecondClick);
        rendered.Find("th").GetAttribute("aria-sort").Should().Be("descending");
    }

    [Fact]
    public void Sorting_parameter_change_is_applied_after_columns_register_not_before()
    {
        // Arrange — applying during the parameter pass would reduce the sorting away, because the column it
        // names only registers while the table renders
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Columns, SortableColumns("col")));

        // Act — one batch adds the column and the sorting naming it
        rendered.Render(b => b
            .Add(p => p.Columns, SortableColumns("col", "late"))
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("late", ascending: true)));

        // Assert
        rendered.WaitForAssertion(() =>
        {
            provider.LastContext.Should().NotBeNull();
            provider.LastContext.SortingState.ColumnSortings.Should().ContainSingle()
                .Which.ColumnId.Should().Be("late");
        });
    }

    [Fact]
    public void Sorting_by_a_header_click_survives_an_unrelated_re_render()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .AddSortableColumn("col"));

        rendered.Find(".sortable").Click();
        rendered.WaitForAssertion(() => rendered.Find("th").GetAttribute("aria-sort").Should().Be("ascending"));

        var callsAfterSort = provider.CallCount;

        // Act
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert
        provider.CallCount.Should().Be(callsAfterSort);
        rendered.Find("th").GetAttribute("aria-sort").Should().Be("ascending");
    }

    [Fact]
    public void Header_click_raises_once_and_refetches_once()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, _ => raised++))
            .AddSortableColumn("col"));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Find(".sortable").Click();

        // Assert
        rendered.WaitForAssertion(() => raised.Should().Be(1));
        provider.CallCount.Should().Be(callsAfterInit + 1);
    }

    [Fact]
    public async Task Resetting_the_sorting_from_outside_applies()
    {
        // Arrange — a Push carries the "I mean it" signal a re-supplied value never could
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("col", ascending: true))
            .AddSortableColumn("col"));

        var callsBeforeReset = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(SortingState.Empty));

        // Assert
        provider.CallCount.Should().Be(callsBeforeReset + 1);
        rendered.Find("th").GetAttribute("aria-sort").Should().Be("none");
    }

    [Fact]
    public async Task Sorting_for_a_column_the_table_does_not_have_is_dropped()
    {
        // Arrange — an unreachable sorting must not order the rows, so it is dropped rather than kept
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .AddSortableColumn("col"));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(
            SortingState.Empty.WithColumnSorting("gone", ascending: true)));

        // Assert
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.SortingState.ColumnSortings.Should().BeEmpty();
        raised.Should().BeNull();
    }

    [Fact]
    public void Parameter_sorting_for_a_column_the_table_does_not_have_is_dropped()
    {
        // Arrange & Act
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("gone", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .AddSortableColumn("col"));

        // Assert — nothing was applied, so nothing is raised either
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.SortingState.ColumnSortings.Should().BeEmpty();
        raised.Should().BeNull();
    }

    [Fact]
    public async Task Dropping_a_sorting_for_an_absent_column_logs_a_warning()
    {
        // Arrange — the drop is silent everywhere else, so the log is what tells a consumer its sort went
        // nowhere. Separate wiring from the filter counterpart, hence its own test.
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RecordingItemsProvider([new(1, "Value")]))
            .AddSortableColumn("col"));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(
            SortingState.Empty.WithColumnSorting("gone", ascending: true)));

        // Assert
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Warning);
    }

    [Fact]
    public async Task Hiding_a_sorted_column_drops_its_sorting_and_logs_nothing()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RecordingItemsProvider([new(1, "Value")]))
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("colA", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        // Act
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Assert
        rendered.WaitForAssertion(() =>
        {
            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().BeEmpty();
        });

        logger.ReceivedCalls()
            .Should().NotContain(call => call.GetMethodInfo().Name == nameof(ILogger.Log));
    }

    [Fact]
    public void Shift_clicking_an_unsorted_header_appends_it_as_the_lowest_priority()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("first", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .Add(p => p.Columns, SortableColumns("first", "second")));

        // Act
        rendered.FindAll(".sortable")[1].Click(new MouseEventArgs { ShiftKey = true });

        // Assert
        rendered.WaitForAssertion(() =>
        {
            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().Equal([
                new ColumnSorting("first", Ascending: true),
                new ColumnSorting("second", Ascending: true)
            ]);
        });
    }

    [Fact]
    public void Shift_clicking_a_sorted_header_inverts_it_and_keeps_its_priority()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty
                .WithColumnSorting("first", ascending: true)
                .WithColumnSorting("second", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .Add(p => p.Columns, SortableColumns("first", "second")));

        // Act
        rendered.FindAll(".sortable")[0].Click(new MouseEventArgs { ShiftKey = true });

        // Assert
        rendered.WaitForAssertion(() =>
        {
            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().Equal([
                new ColumnSorting("first", Ascending: false),
                new ColumnSorting("second", Ascending: true)
            ]);
        });
    }

    [Fact]
    public void Clicking_a_header_without_shift_drops_every_other_column_sorting()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty
                .WithColumnSorting("first", ascending: true)
                .WithColumnSorting("second", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .Add(p => p.Columns, SortableColumns("first", "second")));

        // Act
        rendered.FindAll(".sortable")[1].Click();

        // Assert
        rendered.WaitForAssertion(() =>
        {
            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().ContainSingle()
                .Which.Should().Be(new ColumnSorting("second", Ascending: false));
        });
    }

    [Fact]
    public void Shift_enter_on_a_header_appends_it_like_a_shift_click()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("first", ascending: true))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .Add(p => p.Columns, SortableColumns("first", "second")));

        // Act
        rendered.FindAll(".sortable")[1].KeyUp(new KeyboardEventArgs { Key = "Enter", ShiftKey = true });

        // Assert
        rendered.WaitForAssertion(() =>
        {
            raised.Should().NotBeNull();
            raised.ColumnSortings.Should().Equal([
                new ColumnSorting("first", Ascending: true),
                new ColumnSorting("second", Ascending: true)
            ]);
        });
    }

    [Fact]
    public void Shift_clicking_a_header_of_a_column_that_is_not_sortable_changes_nothing()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        SortingState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, sorting => raised = sorting))
            .AddNonSortableColumn("col"));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Find(".header-cell").Click(new MouseEventArgs { ShiftKey = true });

        // Assert
        raised.Should().BeNull();
        provider.CallCount.Should().Be(callsAfterInit);
    }

    // Several sortable columns in document order, so a test can register one of them late.
    private static RenderFragment SortableColumns(params string[] columnIds)
        => builder =>
        {
            var sequence = 0;

            foreach (var columnId in columnIds)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence++);
                {
                    builder.AddComponentParameter(sequence++, nameof(AdvancedTableColumn<>.Id), columnId);
                    builder.AddComponentParameter(sequence++, nameof(AdvancedTableColumn<>.Sortable), true);
                }
                builder.CloseComponent();
            }
        };
}
