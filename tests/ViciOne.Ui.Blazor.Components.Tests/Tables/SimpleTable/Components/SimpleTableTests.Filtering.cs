using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

// SimpleTable forwards the filter state parameter and re-raises the change event; the wrapped AdvancedTable
// holds the only copy. These guard that the wrapper adds no state of its own.
public sealed partial class SimpleTableTests
{
    [Fact]
    public void Round_trips_a_bound_filter_state_without_re_raising()
    {
        // Arrange — the round trip a bound parent performs on every render has to terminate at the wrapper too
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };
        var raised = 0;
        FilterState? lastRaised = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, filterState =>
            {
                raised++;
                lastRaised = filterState;
            }))
            .Add(p => p.Columns, ValueColumn()));

        rendered.Render(b => b
            .Add(p => p.FilterState, FilterState.Empty.WithColumnFilter(
                new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", "ban", x => x.TestValue))));

        rendered.WaitForAssertion(() =>
        {
            lastRaised.Should().NotBeNull();
            rendered.FindAll("tbody tr").Should().ContainSingle();
        });

        var raisedAfterApply = raised;

        // Act — the parent hands the wrapper's own value back
        rendered.Render(b => b
            .Add(p => p.FilterState, lastRaised));

        // Assert — nothing re-raises and the rows stay as they were
        raised.Should().Be(raisedAfterApply);
        rendered.FindAll("tbody tr").Should().ContainSingle();
    }

    [Fact]
    public async Task Unchanged_filter_state_instance_is_ignored_after_the_table_moved_its_own_state()
    {
        // Arrange — the starting-value pattern through the wrapper: one instance, never replaced. A copy of the
        // filter state held here would put the table back on the next unrelated render.
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };
        var seed = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", "a", x => x.TestValue));

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.FilterState, seed)
            .Add(p => p.Columns, ValueColumn()));

        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().HaveCount(2));

        // The table moves its own state to something the seed does not match
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", "ban", x => x.TestValue))));

        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().ContainSingle());

        // Act — an unrelated re-render hands the same seed instance over again
        rendered.Render(b => b
            .Add(p => p.MaximumHeight, "500px"));

        // Assert — the seed does not put the table back where it started
        rendered.FindAll("tbody tr").Should().ContainSingle();
    }

    private static RenderFragment ValueColumn()
        => builder =>
        {
            builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
            {
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)ValueCell);
            }
            builder.CloseComponent();
        };

    private static RenderFragment ValueCell(TableTestItem item)
        => builder => builder.AddContent(0, item.TestValue);
}
