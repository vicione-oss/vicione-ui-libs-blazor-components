using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public async Task Dropping_a_column_on_another_reorders_them()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second");
        ColumnOrder(renderedComponent).Should().Equal("first", "second");

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[0].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("second", "first"));
    }

    [Fact]
    public async Task Dropping_on_an_end_zone_inserts_the_column_behind_that_column()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[1].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("second", "first", "third"));
    }

    [Fact]
    public async Task Dropping_on_the_end_zone_of_the_last_column_moves_the_column_to_the_last_position()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[2].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("second", "third", "first"));
    }

    [Fact]
    public async Task Dropping_on_the_end_zone_of_the_preceding_column_keeps_the_order()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[0].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("first", "second", "third"));
    }

    [Fact]
    public async Task Dropping_on_the_start_zone_of_the_dragged_column_keeps_the_order()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("first", "second", "third"));
    }

    [Fact]
    public async Task Dropping_on_the_end_zone_of_the_dragged_column_keeps_the_order()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[1].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("first", "second", "third"));
    }

    [Fact]
    public async Task Dropping_on_the_start_zone_of_the_following_column_keeps_the_order()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[2].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => ColumnOrder(renderedComponent).Should().Equal("first", "second", "third"));
    }

    [Fact]
    public async Task Both_zones_of_a_gap_put_the_column_on_the_same_side_of_a_hidden_column()
    {
        // Arrange
        var enteredFromTheLeft = RenderReorderableTable(("first", true), ("hidden", false), ("second", true),
            ("third", true));

        var enteredFromTheRight = RenderReorderableTable(("first", true), ("hidden", false), ("second", true),
            ("third", true));

        // Act
        await enteredFromTheLeft.FindAll("th")[2].DragStartAsync(new DragEventArgs());
        await enteredFromTheLeft.FindAll("th .gap-zone-end")[0].DropAsync(new DragEventArgs());

        await enteredFromTheRight.FindAll("th")[2].DragStartAsync(new DragEventArgs());
        await enteredFromTheRight.FindAll("th .gap-zone-start")[1].DropAsync(new DragEventArgs());

        enteredFromTheLeft.Render(b => b
            .Add(p => p.Columns, ReorderableColumns(("first", true), ("hidden", true), ("second", true), ("third", true))));

        enteredFromTheRight.Render(b => b
            .Add(p => p.Columns, ReorderableColumns(("first", true), ("hidden", true), ("second", true), ("third", true))));

        // Assert
        enteredFromTheLeft.WaitForAssertion(() => ColumnOrder(enteredFromTheLeft).Should()
            .Equal("first", "hidden", "third", "second"));

        enteredFromTheRight.WaitForAssertion(() => ColumnOrder(enteredFromTheRight).Should()
            .Equal("first", "hidden", "third", "second"));
    }

    [Fact]
    public async Task Both_zones_of_a_gap_mark_the_same_column_with_one_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());

        // Act
        await renderedComponent.FindAll("th .gap-zone-end")[1].DragEnterAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            renderedComponent.FindAll("th")[2].ClassList.Should().Contain("gap-indicator-start");
            renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().ContainSingle();
        });

        // Act
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragEnterAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            renderedComponent.FindAll("th")[2].ClassList.Should().Contain("gap-indicator-start");
            renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().ContainSingle();
        });
    }

    [Fact]
    public async Task Gap_zones_exist_only_while_a_column_is_dragged()
    {
        // Arrange: at rest the zones would cover the header, swallowing sorting, filtering and resizing
        var renderedComponent = RenderReorderableTable("first", "second");
        renderedComponent.FindAll(".gap-zone").Should().BeEmpty();

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll(".gap-zone").Should().HaveCount(4));

        // Act
        await renderedComponent.FindAll("th")[0].DragEndAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll(".gap-zone").Should().BeEmpty());
    }

    [Fact]
    public async Task Dragging_into_a_gap_zone_marks_its_gap_with_an_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second");

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start"));
    }

    [Fact]
    public async Task Ending_a_drag_clears_the_gap_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second");
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());
        renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start");

        // Act
        await renderedComponent.FindAll("th")[1].DragEndAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().BeEmpty());
    }

    [Fact]
    public async Task Leaving_a_gap_zone_clears_the_gap_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second");
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());
        renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start");

        // Act
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragLeaveAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().BeEmpty());
    }

    [Fact]
    public async Task Moving_between_the_two_zones_of_one_gap_keeps_the_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[1].DragEnterAsync(new DragEventArgs());

        // Act: the browser enters the new zone before it leaves the old one
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragEnterAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-end")[1].DragLeaveAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            renderedComponent.FindAll("th")[2].ClassList.Should().Contain("gap-indicator-start");
            renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().ContainSingle();
        });
    }

    [Fact]
    public async Task Moving_to_a_zone_of_another_gap_moves_the_indicator()
    {
        // Arrange
        var renderedComponent = RenderReorderableTable("first", "second", "third");
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragEnterAsync(new DragEventArgs());

        // Act
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragLeaveAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start");
            renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().ContainSingle();
        });
    }

    private static IReadOnlyList<string?> ColumnOrder(
        IRenderedComponent<AdvancedTable<TableTestItem>> renderedComponent)
        => [.. renderedComponent.FindAll("th").Select(header => header.GetAttribute("data-column-id"))];

    private IRenderedComponent<AdvancedTable<TableTestItem>> RenderReorderableTable(
        params string[] columnIds)
        => RenderReorderableTable(Array.ConvertAll(columnIds, columnId => (columnId, true)));

    private IRenderedComponent<AdvancedTable<TableTestItem>> RenderReorderableTable(
        params (string ColumnId, bool Visible)[] columns)
        => _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, ReorderableColumns(columns)));

    // Keyed so flipping Visible updates the existing column instead of replacing it — a replacement would
    // re-register the column at the end of the list, losing the order under test.
    private static RenderFragment ReorderableColumns(params (string ColumnId, bool Visible)[] columns)
        => builder =>
        {
            var sequence = 0;

            foreach (var (columnId, visible) in columns)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence);
                {
                    builder.SetKey(columnId);
                    builder.AddComponentParameter(sequence + 1, nameof(AdvancedTableColumn<>.Id), columnId);
                    builder.AddComponentParameter(sequence + 2, nameof(AdvancedTableColumn<>.Title), columnId);
                    builder.AddComponentParameter(sequence + 3, nameof(AdvancedTableColumn<>.Visible), visible);
                }
                builder.CloseComponent();

                sequence += 4;
            }
        };
}
