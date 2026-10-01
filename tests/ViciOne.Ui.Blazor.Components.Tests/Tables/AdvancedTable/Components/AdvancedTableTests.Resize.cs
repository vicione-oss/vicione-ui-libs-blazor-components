using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

// bUnit neither runs the TypeScript module nor has a layout engine, so what is asserted here is the contract
// the two sides meet on: what the colgroup tells TypeScript, what a commit stores, and when the table asks
// for another layout.
public sealed partial class AdvancedTableTests
{
    private const string TableModulePath = "./_content/ViciOne.Ui.Blazor.Components/advanced-table/components/advanced-table.js";

    private const string UnknownColumnId = "no-such-column";

    [Fact]
    public void Resize_handle_present_on_every_resizable_column_including_the_last()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Resizeable = true }, new("col2") { Resizeable = true }])));

        // Assert — the push model gives the last column its own handle
        var headers = rendered.FindAll("th");
        headers[0].QuerySelector(".resize-handle").Should().NotBeNull();
        headers[1].QuerySelector(".resize-handle").Should().NotBeNull();
    }

    [Fact]
    public void Resize_handle_absent_when_resizable_is_false()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Resizeable = false }, new("col2")])));

        // Assert
        rendered.FindAll("th")[0].QuerySelector(".resize-handle").Should().BeNull();
    }

    [Fact]
    public void Resize_handle_absent_on_fixed_column_types()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<TableNavigationColumn<TableTestItem>>(0);
                builder.CloseComponent();

                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(1);
                {
                    builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Id), "col1");
                }
                builder.CloseComponent();
            }));

        // Assert — the navigation column is a fixed type: no handle, no data-min-width, fixed 56px
        rendered.FindAll("th")[0].QuerySelector(".resize-handle").Should().BeNull();
        var navigationCol = rendered.FindAll("col")[0];
        navigationCol.GetAttribute("data-min-width").Should().BeNull();
        navigationCol.GetAttribute("style").Should().Be("width: 56px");
    }

    [Fact]
    public void Col_carries_data_min_width_attribute_matching_column_min_width()
    {
        // Arrange
        const int MinWidth = 80;

        // Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col") { MinimumWidth = MinWidth }])));

        // Assert
        rendered.Find("col").GetAttribute("data-min-width").Should().Be("80");
    }

    [Fact]
    public void Col_carries_the_column_id_it_is_committed_under()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1"), new("col2")])));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].GetAttribute("data-column-id").Should().Be("col1");
        cols[1].GetAttribute("data-column-id").Should().Be("col2");
    }

    [Fact]
    public void Width_seed_below_min_width_renders_at_min_width()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col") { Width = 30, MinimumWidth = 50 }])));

        // Assert
        rendered.Find("col").GetAttribute("style").Should().Be("width: 50px");
    }

    [Fact]
    public void Flex_column_is_marked_as_flex_and_a_width_seeded_one_is_not()
    {
        // Arrange & Act: data-flex is what tells TypeScript which columns it may recompute
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("flex"), new("declared") { Width = 120 }])));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].HasAttribute("data-flex").Should().BeTrue();
        cols[1].HasAttribute("data-flex").Should().BeFalse();
    }

    [Fact]
    public void Fixed_column_types_are_not_marked_as_flex()
    {
        // Arrange & Act: a navigation column reports its own width, so it never takes part in a layout
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<TableNavigationColumn<TableTestItem>>(0);
                builder.CloseComponent();
            }));

        // Assert
        rendered.Find("col").HasAttribute("data-flex").Should().BeFalse();
    }

    [Fact]
    public void Non_resizable_column_is_still_a_flex_column()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("locked") { Resizeable = false, MinimumWidth = 150 }])));

        // Assert
        var col = rendered.Find("col");
        col.HasAttribute("data-flex").Should().BeTrue();
        col.GetAttribute("data-min-width").Should().Be("150");
        rendered.Find("th").QuerySelector(".resize-handle").Should().BeNull();
    }

    [Fact]
    public async Task User_dragged_column_stops_being_a_flex_column()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("dragged"), new("neighbor")])));

        // Act
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "dragged", Value = 200d }, new() { ColumnId = "neighbor", Value = 100d }],
                "dragged"));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].HasAttribute("data-flex").Should().BeFalse();
        cols[1].HasAttribute("data-flex").Should().BeTrue();
    }

    [Fact]
    public async Task Width_a_layout_resolved_leaves_the_column_flex()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1")])));

        // Act
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 200d }], null));

        // Assert
        var col = rendered.Find("col");
        col.GetAttribute("style").Should().Be("width: 200px");
        col.HasAttribute("data-flex").Should().BeTrue();
    }

    [Fact]
    public async Task A_resolved_commit_does_not_release_a_user_drag()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1")])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 200d }], "col1"));

        // Act
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 400d }], null));

        // Assert
        var col = rendered.Find("col");
        col.GetAttribute("style").Should().Be("width: 200px");
        col.HasAttribute("data-flex").Should().BeFalse();
    }

    [Fact]
    public async Task SetColumnWidths_applies_pixel_widths_to_col_styles()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1"), new("col2")])));

        // Act — invoke on the component dispatcher so StateHasChanged() is legal
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "col1", Value = 200d }, new() { ColumnId = "col2", Value = 100d }], null));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].GetAttribute("style").Should().Contain("width: 200px");
        cols[1].GetAttribute("style").Should().Contain("width: 100px");
    }

    [Fact]
    public async Task Table_width_is_summed_px_only_once_every_column_is_pinned()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1"), new("col2")])));

        // Assert — unresolved flex keeps the table at 100%
        rendered.Find("table").GetAttribute("style").Should().Be("width: 100%");

        // Act — pinning only one column must not flip the width mechanism
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 200d }], null));

        rendered.Find("table").GetAttribute("style").Should().Be("width: 100%");

        // Act — pinning the second column completes the set
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col2", Value = 100d }], null));

        // Assert
        rendered.Find("table").GetAttribute("style").Should().Be("width: 300px");
    }

    [Fact]
    public async Task SetColumnWidths_ignores_keys_of_unknown_columns()
    {
        // Arrange — a width is committed for a key no column holds (e.g. the column was removed mid-drag)
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1")])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "col1", Value = 200d }, new() { ColumnId = UnknownColumnId, Value = 100d }], null));

        // Act — a second column appears afterwards
        rendered.Render(b => b
            .Add(p => p.Columns, SizedColumns([new("col1"), new("col2")])));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
        {
            var cols = rendered.FindAll("col");
            cols.Should().HaveCount(2);
            cols[0].GetAttribute("style").Should().Contain("width: 200px");
            cols[1].GetAttribute("style").Should().BeNull();
        });
    }

    [Fact]
    public async Task Removing_resized_column_releases_its_width()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1"), new("col2")])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "col1", Value = 200d }, new() { ColumnId = "col2", Value = 100d }], "col1"));

        rendered.Find("table").GetAttribute("style").Should().Be("width: 300px");

        // Act
        rendered.Render(b => b
            .Add(p => p.Columns, SizedColumns([new("col1")])));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            rendered.Find("table").GetAttribute("style").Should().Be("width: 200px"));
    }

    [Fact]
    public async Task Hiding_pinned_column_shrinks_table_to_visible_width()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" }])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "col1", Value = 200d }, new() { ColumnId = "col2", Value = 100d }], null));

        rendered.Find("table").GetAttribute("style").Should().Be("width: 300px");

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: false);

        // Assert
        rendered.Find("table").GetAttribute("style").Should().Be("width: 200px");
        rendered.FindAll("col").Should().ContainSingle();
    }

    [Fact]
    public async Task Showing_hidden_column_restores_table_width()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" }])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync(
                [new() { ColumnId = "col1", Value = 200d }, new() { ColumnId = "col2", Value = 100d }], null));

        await SetColumnVisibilityAsync(rendered, "col2", visible: false);
        rendered.Find("table").GetAttribute("style").Should().Be("width: 200px");

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: true);

        // Assert
        rendered.Find("table").GetAttribute("style").Should().Be("width: 300px");
        rendered.FindAll("col").Should().HaveCount(2);
    }

    [Fact]
    public async Task Hiding_a_user_dragged_column_keeps_its_width_and_its_latch()
    {
        // Arrange
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" }])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col2", Value = 180d }], "col2"));

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: false);
        await SetColumnVisibilityAsync(rendered, "col2", visible: true);

        // Assert
        var col2 = rendered.FindAll("col")[1];
        col2.GetAttribute("style").Should().Be("width: 180px");
        col2.HasAttribute("data-flex").Should().BeFalse();
    }

    [Fact]
    public async Task Hiding_the_only_unresolved_flex_column_pins_table_to_visible_width()
    {
        // Arrange: a column without a committed width keeps the table at 100%, because its <col> renders no
        // width for the summed total to include
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" }])));

        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 200d }], null));

        rendered.Find("table").GetAttribute("style").Should().Be("width: 100%");

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: false);

        // Assert
        rendered.Find("table").GetAttribute("style").Should().Be("width: 200px");
    }

    [Fact]
    public async Task Attaching_lays_the_columns_out_without_a_separate_notification()
    {
        // Arrange & Act
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().BeEmpty());
    }

    [Fact]
    public async Task Committing_widths_does_not_ask_for_another_layout()
    {
        // Arrange: laying out right after a drag would re-flow the dragged column's neighbors
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        // Act
        await rendered.InvokeAsync(() =>
            rendered.Instance.SetColumnWidthsAsync([new() { ColumnId = "col1", Value = 200d }], "col1"));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().BeEmpty());
    }

    [Fact]
    public async Task Hiding_a_column_asks_for_another_layout()
    {
        // Arrange: a hidden column renders no <col>, so the space the remaining flex columns share changed
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: false);

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().NotBeEmpty());
    }

    [Fact]
    public async Task Showing_a_column_asks_for_another_layout()
    {
        // Arrange
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        await SetColumnVisibilityAsync(rendered, "col2", visible: false);

        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().ContainSingle());

        // Act
        await SetColumnVisibilityAsync(rendered, "col2", visible: true);

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().HaveCount(2));
    }

    [Fact]
    public async Task Adding_a_column_asks_for_another_layout()
    {
        // Arrange
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        // Act
        rendered.Render(b => b
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" },
                new("col3") { Title = "col3" }])));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().NotBeEmpty());
    }

    [Fact]
    public async Task Removing_a_column_asks_for_another_layout()
    {
        // Arrange
        var attachResult = SetupTableModule();

        var rendered = RenderTableWithTwoColumns();

        // Act
        rendered.Render(b => b
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }])));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["columnsChanged"].Should().NotBeEmpty());
    }

    private BunitJSModuleInterop SetupTableModule()
    {
        var module = _testContext.JSInterop.SetupModule(TableModulePath);
        var attachResult = module.SetupModule(invocation => invocation.Identifier == "attach");

        attachResult.SetupVoid("columnsChanged").SetVoidResult();

        return attachResult;
    }

    private static RenderFragment SizedColumns(IReadOnlyList<TestColumn> columns)
        => builder =>
        {
            var sequence = 0;

            foreach (var column in columns)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence);
                {
                    builder.AddComponentParameter(sequence + 1, nameof(AdvancedTableColumn<>.Id), column.Id);

                    if (column.Title is { } title)
                        builder.AddComponentParameter(sequence + 2, nameof(AdvancedTableColumn<>.Title), title);

                    if (column.Width is { } width)
                        builder.AddComponentParameter(sequence + 3, nameof(AdvancedTableColumn<>.Width), width);

                    if (column.MinimumWidth is { } minimumWidth)
                    {
                        builder.AddComponentParameter(
                            sequence + 4, nameof(AdvancedTableColumn<>.MinimumWidth), minimumWidth);
                    }

                    if (column.Resizeable is { } resizeable)
                        builder.AddComponentParameter(sequence + 5, nameof(AdvancedTableColumn<>.Resizeable), resizeable);
                }
                builder.CloseComponent();

                sequence += 6;
            }
        };

    private IRenderedComponent<AdvancedTable<TableTestItem>> RenderTableWithTwoColumns()
        => _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, SizedColumns([new("col1") { Title = "col1" }, new("col2") { Title = "col2" }])));
}
