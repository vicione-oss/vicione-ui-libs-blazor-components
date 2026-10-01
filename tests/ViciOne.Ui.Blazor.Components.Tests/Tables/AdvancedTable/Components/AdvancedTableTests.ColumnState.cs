using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    // Column states are keyed by column instance, so every cell of a column — header and body — must share
    // one ColumnState instance, and that pairing must survive removing another column. A positional pairing
    // breaks here: after a removal every later column shifts onto a different state object.

    [Fact]
    public void Should_keep_column_states_paired_with_their_columns_after_removing_a_column()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, BuildColumns("ColA", "ColB", "ColC")));

        // Act — remove the first column; ColB and ColC shift position but must keep their states
        rendered.Render(b => b
            .Add(p => p.Columns, BuildColumns("ColB", "ColC")));

        // Assert — one distinct state per remaining column, each shared by its header cell and body cells
        var groups = rendered.FindComponents<CellContainer>()
            .GroupBy(c => c.Instance.ColumnState)
            .ToList();

        groups.Should().HaveCount(2);
        groups.Should().AllSatisfy(g => g.Should().HaveCount(items.Count + 1));
    }

    [Fact]
    public void Should_refresh_only_the_cells_of_the_column_whose_refresh_was_requested_after_a_removal()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, BuildColumns("ColA", "ColB", "ColC")));

        rendered.Render(b => b
            .Add(p => p.Columns, BuildColumns("ColB", "ColC")));

        var containers = rendered.FindComponents<CellContainer>();

        // Every one of ColB's cells names the column — the header renders its title, the body cells carry it
        // as their column id — and they all share the one state this refresh is aimed at.
        var stateB = containers.First(c => c.Markup.Contains("ColB", StringComparison.Ordinal)).Instance.ColumnState;

        var countsBefore = containers.ToDictionary(c => c, c => c.RenderCount);

        // Act — targeted update on ColB
        stateB.RequestRefresh();

        // Assert — exactly ColB's cells re-render
        rendered.WaitForAssertion(() => containers
            .Where(c => c.Instance.ColumnState == stateB)
            .Should().AllSatisfy(c => c.RenderCount.Should().BeGreaterThan(countsBefore[c])));

        containers
            .Where(c => c.Instance.ColumnState != stateB)
            .Should().AllSatisfy(c => c.RenderCount.Should().Be(countsBefore[c]));
    }

    [Fact]
    public void Should_pair_a_re_registered_column_with_its_own_state()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, BuildColumns("ColA", "ColB")));

        // Act — unregister ColA, then register it again
        rendered.Render(b => b
            .Add(p => p.Columns, BuildColumns("ColB")));
        rendered.Render(b => b
            .Add(p => p.Columns, BuildColumns("ColB", "ColA")));

        // Assert — the cycle leaves one distinct state per column, correctly shared within each column
        var groups = rendered.FindComponents<CellContainer>()
            .GroupBy(c => c.Instance.ColumnState)
            .ToList();

        groups.Should().HaveCount(2);
        groups.Should().AllSatisfy(g => g.Should().HaveCount(items.Count + 1));
    }

    // Columns are keyed so a removal disposes the removed column component (unregistering it) instead of
    // mutating a positional neighbor's parameters.
    private static RenderFragment BuildColumns(params string[] columnIds)
        => builder =>
        {
            var sequence = 0;

            foreach (var columnId in columnIds)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence);
                builder.SetKey(columnId);
                builder.AddComponentParameter(sequence + 1, nameof(AdvancedTableColumn<>.Id), columnId);
                builder.AddComponentParameter(sequence + 2, nameof(AdvancedTableColumn<>.Title), columnId);
                builder.CloseComponent();

                sequence += 3;
            }
        };
}
