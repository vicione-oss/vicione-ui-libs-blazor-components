using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public void Column_header_renders_draggable_as_the_literal_string_true()
    {
        // Arrange & Act — a bool renders the attribute minimized as draggable="", which the browser reads
        // as "auto" and therefore not draggable, so the value must be the literal string
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .AddSortableColumn("col"));

        // Assert
        rendered.Find("th").GetAttribute("draggable").Should().Be("true");
    }

    [Fact]
    public void Column_header_is_not_draggable_while_its_filter_panel_is_open()
    {
        // Arrange — a drag started inside the panel (selecting text in an input) must not begin a reorder
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");

        // Act
        rendered.Find(".column-filter-button").Click();

        // Assert
        rendered.WaitForAssertion(() => rendered.Find("th").GetAttribute("draggable").Should().Be("false"));
    }

    [Fact]
    public void Clicking_the_filter_icon_opens_the_panel()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");
        rendered.FindAll(".column-filter-panel").Should().BeEmpty();

        // Act
        rendered.Find(".column-filter-button").Click();

        // Assert
        rendered.WaitForAssertion(() => rendered.FindAll(".column-filter-panel").Should().ContainSingle());
    }

    [Fact]
    public void Opening_the_panel_registers_exactly_one_close_observer()
    {
        // Arrange — the observer id is only stored once the interop completes, so a render landing while the
        // registration is still in flight used to re-enter OnAfterRenderAsync and register a second observer
        // whose id nothing ever stops. Leaving the call unresolved holds that window open.
        var startObserveClose = _testContext.JSInterop.Setup<int>("startObserveClose", _ => true);
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");

        // Act
        rendered.Find(".column-filter-button").Click();
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert
        _testContext.JSInterop.Invocations["startObserveClose"].Should().ContainSingle();

        startObserveClose.SetResult(1);
    }

    [Fact]
    public async Task Escape_is_not_handled_by_the_panel()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");
        await rendered.Find(".column-filter-button").ClickAsync(new MouseEventArgs());
        var panel = rendered.WaitForElement(".column-filter-panel");

        // Act
        var keyDown = () => panel.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        await keyDown.Should().ThrowAsync<MissingEventHandlerException>();
        rendered.FindAll(".column-filter-panel").Should().ContainSingle();
    }

    [Fact]
    public async Task Setting_the_filter_state_applies_and_refetches_once()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");
        var callsAfterInit = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit + 1);

        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().ContainSingle();
    }

    [Fact]
    public async Task Setting_the_applied_filter_state_again_neither_raises_nor_refetches()
    {
        // Arrange — a no-op copy-method returns the same instance, which is what makes this detectable
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"));
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, filter)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, _ => raised++))
            .Add(p => p.Columns, FilterColumn("col")));

        var callsAfterInit = provider.CallCount;
        var raisedAfterInit = raised;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(filter));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit);
        raised.Should().Be(raisedAfterInit);
    }

    [Fact]
    public void Parameter_instance_change_applies_and_raises()
    {
        // Arrange — a new instance is a command, honored for the whole lifetime of the table
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        FilterState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged,
                EventCallback.Factory.Create<FilterState>(this, filterState => raised = filterState))
            .Add(p => p.Columns, FilterColumn("col")));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Render(b => b
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        // Assert
        rendered.WaitForAssertion(() =>
        {
            provider.CallCount.Should().Be(callsAfterInit + 1);

            raised.Should().NotBeNull();
            raised.Filters.Should().ContainSingle();

            provider.LastContext.Should().NotBeNull();
            provider.LastContext.FilterState.Filters.Should().ContainSingle();
        });
    }

    [Fact]
    public void Unchanged_parameter_instance_is_ignored_after_the_table_moved_its_own_state()
    {
        // Arrange — the starting-value pattern: one instance, handed over on every render and never replaced
        var seed = FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"));
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, seed)
            .Add(p => p.Columns, FilterColumns("col", "other")));

        // The table moves its own state, or the re-render below would prove nothing
        rendered.FindAll(".column-filter-button")[1].Click();
        rendered.WaitForElement(".apply-test-filter").Click();

        rendered.WaitForAssertion(() =>
        {
            provider.LastContext.Should().NotBeNull();
            provider.LastContext.FilterState.Filters.Should().HaveCount(2);
        });

        var callsAfterHeaderFilter = provider.CallCount;

        // Act — an unrelated re-render hands the same instance over again
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the seed does not put the table back where it started
        provider.CallCount.Should().Be(callsAfterHeaderFilter);

        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().HaveCount(2);
    }

    [Fact]
    public void Own_value_fed_back_as_parameter_neither_raises_nor_refetches()
    {
        // Arrange — what a correctly bound parent does on every render; the round trip has to terminate
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;
        FilterState? lastRaised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, filterState =>
            {
                raised++;
                lastRaised = filterState;
            }))
            .Add(p => p.Columns, FilterColumn("col")));

        rendered.Find(".column-filter-button").Click();
        rendered.WaitForElement(".apply-test-filter").Click();

        rendered.WaitForAssertion(() => lastRaised.Should().NotBeNull());

        var callsAfterHeaderFilter = provider.CallCount;
        var raisedAfterHeaderFilter = raised;

        // Act
        rendered.Render(b => b
            .Add(p => p.FilterState, lastRaised));

        // Assert
        provider.CallCount.Should().Be(callsAfterHeaderFilter);
        raised.Should().Be(raisedAfterHeaderFilter);
    }

    [Fact]
    public async Task Parameter_ignored_by_a_guard_is_still_recorded_as_last_received()
    {
        // Arrange — a parent that binds but stops handling the event. Its value is rejected as the table's own
        // on the way in; unless that instance is recorded anyway, the very same value reads as a fresh command
        // once the table has moved on, and silently reverts the user.
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Columns, FilterColumn("col")));

        var firstFilter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"));
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(firstFilter));

        // The parent hands the table's own value back — rejected by the own-value guard, recorded regardless
        rendered.Render(b => b
            .Add(p => p.FilterState, firstFilter));

        // The table moves on again and the parent does not follow along
        var secondFilter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"));
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(secondFilter));

        var callsAfterSecondFilter = provider.CallCount;

        // Act — an unrelated re-render, still carrying the parent's stale value
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the stale value is not mistaken for a fresh command, so the second filter survives
        provider.CallCount.Should().Be(callsAfterSecondFilter);

        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Should().BeSameAs(secondFilter);
    }

    [Fact]
    public void Parameter_change_is_applied_after_columns_register_not_before()
    {
        // Arrange — applying during the parameter pass would reduce the filter away, because the column it
        // names only registers while the table renders
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Columns, FilterColumns("col")));

        // Act — one batch adds the column and the filter naming it
        rendered.Render(b => b
            .Add(p => p.Columns, FilterColumns("col", "late"))
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("late"))));

        // Assert
        rendered.WaitForAssertion(() =>
        {
            provider.LastContext.Should().NotBeNull();
            provider.LastContext.FilterState.Filters.Should().ContainSingle()
                .Which.Should().BeOfType<TestColumnFilter>()
                .Which.ColumnId.Should().Be("late");
        });
    }

    [Fact]
    public void Filter_and_sorting_changed_in_one_batch_refetch_once()
    {
        // Arrange — both apply before the fetch, so a batch that moves each of them costs one provide
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        List<string> raisedEvents = [];

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, _ => raisedEvents.Add("filter")))
            .Add(p => p.SortingStateChanged, EventCallback.Factory.Create<SortingState>(this, _ => raisedEvents.Add("sorting")))
            .Add(p => p.Columns, SortableFilterColumn("col")));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Render(b => b
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col")))
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("col", ascending: true)));

        // Assert — one fetch, and the filter is reported before the sorting
        rendered.WaitForAssertion(() =>
        {
            provider.CallCount.Should().Be(callsAfterInit + 1);
            raisedEvents.Should().Equal(["filter", "sorting"]);
        });
    }

    [Fact]
    public async Task Two_set_calls_refetch_twice()
    {
        // Arrange — the accepted cost of having no combined setter: each call fetches for itself
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Columns, SortableFilterColumn("col")));

        var callsAfterInit = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));
        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(
            SortingState.Empty.WithColumnSorting("col", ascending: true)));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit + 2);
    }

    [Fact]
    public void Fresh_instance_per_render_without_a_binding_reapplies_each_render()
    {
        // Arrange — the accepted misuse in its terminating form. Nothing feeds the change back here, so it
        // stops after one render; under a binding the same code never stops.
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col")))
            .Add(p => p.Columns, FilterColumn("col")));

        var callsAfterInit = provider.CallCount;

        // Act — a distinct instance carrying the same filter
        rendered.Render(b => b
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        // Assert
        rendered.WaitForAssertion(() => provider.CallCount.Should().Be(callsAfterInit + 1));
    }

    [Fact]
    public async Task Filter_for_a_column_the_table_does_not_have_is_dropped()
    {
        // Arrange — an unreachable filter must not narrow the rows, so it is dropped rather than kept
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        FilterState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, filter => raised = filter))
            .Add(p => p.Columns, FilterColumn("col")));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("gone"))));

        // Assert
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().BeEmpty();
        raised.Should().BeNull();
    }

    [Fact]
    public void Parameter_filter_for_a_column_the_table_does_not_have_is_dropped()
    {
        // Arrange & Act
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        FilterState? raised = null;

        _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("gone")))
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, filter => raised = filter))
            .Add(p => p.Columns, FilterColumn("col")));

        // Assert — nothing was applied, so nothing is raised either
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().BeEmpty();
        raised.Should().BeNull();
    }

    [Fact]
    public async Task Global_filter_survives_a_column_filter_being_dropped()
    {
        // Arrange — a global filter is keyed by its type, not a column, so no column can make it unreachable
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");

        var pushed = FilterState.Empty
            .WithGlobalFilter(new TestGlobalFilter("Value"))
            .WithColumnFilter(new TestColumnFilter("gone"));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(pushed));

        // Assert
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().ContainSingle().Which.Should().BeOfType<TestGlobalFilter>();
    }

    [Fact]
    public void Table_without_columns_still_loads_its_items()
    {
        // Arrange & Act — the first provide waits for columns, and a table registering none still has to load
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider));

        // Assert
        provider.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Applying_a_filter_unbound_then_an_unrelated_re_render_neither_refetches_nor_prunes()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var provider = new CountingFilterableProvider([item1, item2]);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged,
                EventCallback.Factory.Create<List<TableTestItem>>(this, items => received = items))
            .Add(p => p.Columns, FilterColumn("col")));

        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { CtrlKey = true });
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { CtrlKey = true });
        received.Should().BeEquivalentTo([item1, item2]);

        // Act — apply a filter from the column header (FilterStateChanged is unbound → self-managed), then let the
        // parent re-render for an unrelated reason
        await rendered.Find(".column-filter-button").ClickAsync(new MouseEventArgs());
        await rendered.WaitForElement(".apply-test-filter").ClickAsync(new MouseEventArgs());

        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().ContainSingle());

        var callsAfterApply = provider.CallCount;

        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the filter hides item2 but must never deselect it, and the re-render neither refetches
        // nor prunes the selection
        provider.CallCount.Should().Be(callsAfterApply);
        received.Should().BeEquivalentTo([item1, item2]);
    }

    [Fact]
    public void Applying_a_filter_in_a_column_header_raises_once_and_refetches_once()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var raised = 0;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, _ => raised++))
            .Add(p => p.Columns, FilterColumn("col")));

        var callsAfterInit = provider.CallCount;

        // Act
        rendered.Find(".column-filter-button").Click();
        rendered.WaitForElement(".apply-test-filter").Click();

        // Assert
        rendered.WaitForAssertion(() => raised.Should().Be(1));
        provider.CallCount.Should().Be(callsAfterInit + 1);
    }

    [Fact]
    public async Task Clearing_the_filter_from_outside_applies()
    {
        // Arrange — a Push carries the "I mean it" signal a re-supplied value never could
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col")))
            .Add(p => p.Columns, FilterColumn("col")));

        var callsBeforeClear = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(FilterState.Empty));

        // Assert
        provider.CallCount.Should().Be(callsBeforeClear + 1);

        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Filters.Should().BeEmpty();
    }

    [Fact]
    public async Task Clearing_an_already_clear_filter_does_nothing()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        var rendered = RenderTableWithFilterColumn(provider, "col");
        var callsAfterInit = provider.CallCount;

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(FilterState.Empty));

        // Assert
        provider.CallCount.Should().Be(callsAfterInit);
    }

    [Fact]
    public async Task Dropping_one_of_several_filters_raises_the_reduced_value()
    {
        // Arrange — the only path on which a drop reaches the callback: what survives has to be what is
        // raised, or a consumer persisting the callback stores a filter the table is not applying
        var provider = new RecordingItemsProvider([new(1, "Value")]);
        FilterState? raised = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, filter => raised = filter))
            .Add(p => p.Columns, FilterColumn("col")));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("col"))
            .WithColumnFilter(new TestColumnFilter("gone"))));

        // Assert
        raised.Should().NotBeNull();
        raised.Filters.Should().ContainSingle()
            .Which.Should().BeOfType<TestColumnFilter>()
            .Which.ColumnId.Should().Be("col");

        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Should().BeSameAs(raised);
    }

    [Fact]
    public async Task Dropping_a_filter_for_an_absent_column_logs_a_warning()
    {
        // Arrange — the log is the only signal a consumer gets that its filter went nowhere
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        var rendered = RenderTableWithFilterColumn(new RecordingItemsProvider([new(1, "Value")]), "col");

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("gone"))));

        // Assert
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Warning);
    }

    [Fact]
    public async Task Hiding_a_filtered_column_logs_nothing()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new CountingFilterableProvider([new(1, "A"), new(2, "B")]))
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("colA")))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        // Act
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Assert
        logger.ReceivedCalls()
            .Should().NotContain(call => call.GetMethodInfo().Name == nameof(ILogger.Log));
    }

    [Fact]
    public void An_items_provider_swap_keeps_the_applied_filter_and_raises_nothing()
    {
        // Arrange — a swap changes where rows come from, not which view of them the user chose
        var raised = 0;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RecordingItemsProvider([new(1, "Value")]))
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(new TestColumnFilter("col")))
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, _ => raised++))
            .Add(p => p.Columns, FilterColumn("col")));

        var raisedAfterInit = raised;
        var swapped = new RecordingItemsProvider([new(2, "Other")]);

        // Act
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, swapped));

        // Assert — the new provider is asked once, and asked for the same view
        swapped.CallCount.Should().Be(1);
        raised.Should().Be(raisedAfterInit);

        swapped.LastContext.Should().NotBeNull();
        swapped.LastContext.FilterState.Filters.Should().ContainSingle()
            .Which.Should().BeOfType<TestColumnFilter>()
            .Which.ColumnId.Should().Be("col");
    }

    [Fact]
    public async Task Hiding_a_filtered_column_clears_its_filter_and_unfilters_the_data()
    {
        // Arrange — seed a filter on colA (colB stays visible so rows keep rendering after colA is hidden)
        var provider = new CountingFilterableProvider([new(1, "A"), new(2, "B")]);
        FilterState? emitted = null;

        var seed = FilterState.Empty.WithColumnFilter(new TestColumnFilter("colA"));

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.FilterState, seed)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, f => emitted = f))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().ContainSingle());

        // Act — hide the filtered column the way the column chooser does
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Assert
        rendered.WaitForAssertion(() =>
        {
            rendered.FindAll("tbody tr").Should().HaveCount(2);
            emitted.Should().NotBeNull();
            emitted.Filters.OfType<IColumnFilter>().Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Hiding_a_column_leaves_other_columns_and_global_filters_untouched()
    {
        // Arrange — seed a column filter for each of two columns plus a global filter
        var provider = new CountingFilterableProvider([new(1, "A"), new(2, "B")]);
        FilterState? emitted = null;

        var seed = FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("colA"))
            .WithColumnFilter(new TestColumnFilter("colB"))
            .WithGlobalFilter(new TestGlobalFilter("x"));

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.FilterState, seed)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, f => emitted = f))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        // Act — hide only colA
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Assert — colA's filter is dropped; colB's column filter and the global filter remain
        rendered.WaitForAssertion(() =>
        {
            emitted.Should().NotBeNull();
            emitted.Filters.OfType<IColumnFilter>().Select(f => f.ColumnId).Should().ContainSingle()
                .Which.Should().Be("colB");
            emitted.Filters.OfType<TestGlobalFilter>().Should().ContainSingle();
        });
    }

    [Fact]
    public async Task Re_showing_a_hidden_column_does_not_restore_its_filter()
    {
        // Arrange — seed a filter on colA, then hide colA (which clears it)
        var provider = new CountingFilterableProvider([new(1, "A"), new(2, "B")]);
        FilterState? emitted = null;

        var seed = FilterState.Empty.WithColumnFilter(new TestColumnFilter("colA"));

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.FilterState, seed)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, f => emitted = f))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().ContainSingle());

        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Act — show the column again
        await SetColumnVisibilityAsync(rendered, "colA", visible: true);

        // Assert — the previously-cleared filter stays gone and all rows remain
        rendered.WaitForAssertion(() =>
        {
            emitted.Should().NotBeNull();
            emitted.Filters.OfType<IColumnFilter>().Should().BeEmpty();
            rendered.FindAll("tbody tr").Should().HaveCount(2);
        });
    }

    // Two columns registered for the chooser (ShowInColumnChooser defaults to true), no filter UI needed.
    // Each carries its id as its title as well, so a test can address the column by the header the chooser
    // shows and still read as if it were naming the column itself.
    private static RenderFragment ChooserColumns(string firstId, string secondId)
        => builder =>
        {
            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
            builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), firstId);
            builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Title), firstId);
            builder.CloseComponent();

            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(3);
            builder.AddComponentParameter(4, nameof(AdvancedTableColumn<>.Id), secondId);
            builder.AddComponentParameter(5, nameof(AdvancedTableColumn<>.Title), secondId);
            builder.CloseComponent();
        };

    private IRenderedComponent<AdvancedTable<TableTestItem>> RenderTableWithFilterColumn(
        IItemsProvider<TableTestItem> provider, string columnId)
            => _testContext.Render<AdvancedTable<TableTestItem>>(b => b
                .Add(p => p.ItemsProvider, provider)
                .Add(p => p.Columns, FilterColumn(columnId)));

    // A column carrying a filter editor that renders a button which commits a filter for its column.
    private static RenderFragment FilterColumn(string columnId)
        => builder =>
        {
            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
            {
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), columnId);
                builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.FilterEditor), FilterEditor());
            }
            builder.CloseComponent();
        };

    // Several such columns in document order, so a test can move the applied filter with one header and check
    // that another column's filter is left alone.
    private static RenderFragment FilterColumns(params string[] columnIds)
        => builder =>
        {
            var sequence = 0;

            foreach (var columnId in columnIds)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence++);
                {
                    builder.AddComponentParameter(sequence++, nameof(AdvancedTableColumn<>.Id), columnId);
                    builder.AddComponentParameter(sequence++, nameof(AdvancedTableColumn<>.FilterEditor),
                        FilterEditor());
                }
                builder.CloseComponent();
            }
        };

    // Filterable and sortable at once, for the tests that move both states.
    private static RenderFragment SortableFilterColumn(string columnId)
        => builder =>
        {
            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
            {
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), columnId);
                builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Sortable), true);
                builder.AddComponentParameter(3, nameof(AdvancedTableColumn<>.FilterEditor), FilterEditor());
            }
            builder.CloseComponent();
        };

    // A plain fragment: the editor component reads the column's context off the cascade the panel puts around it,
    // so nothing is passed here.
    private static RenderFragment FilterEditor()
        => builder =>
        {
            builder.OpenComponent<TestApplyingColumnFilterEditor>(0);
            builder.CloseComponent();
        };
}
