using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public void Hiding_a_column_through_its_parameter_removes_it_from_colgroup_and_header()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: true)));

        rendered.FindAll("th").Should().HaveCount(2);
        rendered.FindAll("colgroup col").Should().HaveCount(2);

        // Act — the consumer flips the column's Visible parameter from its own markup
        rendered.Render(b => b
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: false)));

        // Assert
        rendered.WaitForAssertion(() =>
        {
            rendered.FindAll("th").Should().HaveCount(1);
            rendered.FindAll("colgroup col").Should().HaveCount(1);
        });
    }

    [Fact]
    public void Hiding_a_column_through_its_parameter_drops_its_filter()
    {
        // Arrange — a column filter for each of two columns; hiding one must drop only that one
        var provider = new CountingFilterableProvider([new(1, "A"), new(2, "B")]);
        FilterState? emitted = null;

        var seed = FilterState.Empty
            .WithColumnFilter(new TestColumnFilter("colA"))
            .WithColumnFilter(new TestColumnFilter("colB"));

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.FilterState, seed)
            .Add(p => p.FilterStateChanged, EventCallback.Factory.Create<FilterState>(this, f => emitted = f))
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: true)));

        // Act
        rendered.Render(b => b
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: false)));

        // Assert — a filter the user can no longer see or clear must not keep narrowing the rows
        rendered.WaitForAssertion(() =>
        {
            emitted.Should().NotBeNull();
            emitted.Filters.OfType<IColumnFilter>().Select(f => f.ColumnId).Should().ContainSingle()
                .Which.Should().Be("colB");
        });
    }

    [Fact]
    public void Hiding_a_column_through_its_parameter_drops_its_sorting()
    {
        // Arrange
        var provider = new RecordingItemsProvider([new(1, "A"), new(2, "B")]);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("colA", ascending: true))
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: true)));

        // Act
        rendered.Render(b => b
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: false)));

        // Assert — the sorting is keyed to a column that no longer has a header to clear it from
        rendered.WaitForAssertion(() =>
        {
            provider.LastContext.Should().NotBeNull();
            provider.LastContext.SortingState.ColumnSortings.Should().BeEmpty();
        });
    }

    [Fact]
    public void No_data_placeholder_spans_only_the_visible_columns()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, VisibilityColumns("colA", "colB", firstVisible: false)));

        // Assert — counting the hidden column would stretch the placeholder past the table's width
        rendered.WaitForAssertion(()
            => rendered.Find("tr.no-data td").GetAttribute("colspan").Should().Be("1"));
    }

    [Fact]
    public void Initial_render_runs_no_visibility_pass_per_column()
    {
        // Arrange & Act — a column whose state disagreed with its Visible parameter at registration would
        // report a change the table answers with a filter and sorting pass plus a render, once per column. The
        // render count would then grow with the number of columns.
        var twoColumns = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, BuildColumns("colA", "colB")));

        var sixColumns = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, BuildColumns("colA", "colB", "colC", "colD", "colE", "colF")));

        // Assert — the placeholder proves the first provider response is rendered, so any per-column pass has
        // landed by the time the counts are compared.
        twoColumns.WaitForElement("tr.no-data");
        sixColumns.WaitForElement("tr.no-data");

        sixColumns.RenderCount.Should().Be(twoColumns.RenderCount);
    }

    // Two sortable columns, the first one's visibility controlled by the caller the way a consumer sets it
    // from markup.
    private static RenderFragment VisibilityColumns(string firstId, string secondId, bool firstVisible)
        => builder =>
        {
            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
            {
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), firstId);
                builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Sortable), true);
                builder.AddComponentParameter(3, nameof(AdvancedTableColumn<>.Visible), firstVisible);
            }
            builder.CloseComponent();

            builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(4);
            {
                builder.AddComponentParameter(5, nameof(AdvancedTableColumn<>.Id), secondId);
                builder.AddComponentParameter(6, nameof(AdvancedTableColumn<>.Sortable), true);
            }
            builder.CloseComponent();
        };
}
