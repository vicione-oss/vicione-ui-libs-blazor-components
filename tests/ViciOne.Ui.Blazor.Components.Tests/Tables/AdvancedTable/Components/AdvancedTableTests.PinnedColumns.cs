using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public void Left_pinned_column_has_pinned_col_class_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Left))));

        // Assert
        renderedComponent.Find("th").ClassList.Should().Contain("pinned-column");
    }

    [Fact]
    public void Right_pinned_column_has_pinned_col_class_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Right))));

        // Assert
        renderedComponent.Find("th").ClassList.Should().Contain("pinned-column");
    }

    [Fact]
    public void Unpinned_column_does_not_have_pinned_col_class_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.None))));

        // Assert
        renderedComponent.Find("th").ClassList.Should().NotContain("pinned-column");
    }

    [Fact]
    public void Left_pinned_column_has_pin_left_style_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Left))));

        // Assert
        var header = renderedComponent.Find("th");
        header.GetAttribute("style").Should()
            .Contain("left: var(--pin-left-0)");
    }

    [Fact]
    public void Right_pinned_column_has_pin_right_style_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Right))));

        // Assert
        var header = renderedComponent.Find("th");
        header.GetAttribute("style").Should()
            .Contain("right: var(--pin-right-0)");
    }

    [Fact]
    public void Unpinned_column_has_no_pin_offset_style_on_header()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.None))));

        // Assert
        renderedComponent.Find("th").GetAttribute("style").Should().BeNullOrEmpty();
    }

    [Fact]
    public void Pinned_column_id_never_reaches_the_header_style()
    {
        // Arrange & Act — Blazor escapes < > & " ' in an attribute value but not ) or ;, so an id spliced
        // into the style could close the var() call and append declarations of its own.
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("a); position: fixed; --b: var(c", "Hostile", PinSide.Left))));

        // Assert — the id still reaches the DOM, so this cannot pass by dropping it altogether
        var header = renderedComponent.Find("th");

        header.GetAttribute("style").Should().Be("left: var(--pin-left-0)");
        header.GetAttribute("data-column-id").Should().Be("a); position: fixed; --b: var(c");
    }

    [Fact]
    public void Pin_offset_vars_are_named_by_header_position_on_first_render()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left),
                ("none", "C", PinSide.None),
                ("right-a", "D", PinSide.Right))));

        // Assert
        AssertPinVarsMatchHeaderPositions(renderedComponent);

        var headers = renderedComponent.FindAll("th");
        headers[0].GetAttribute("style").Should().Be("left: var(--pin-left-0)");
        headers[1].GetAttribute("style").Should().Be("left: var(--pin-left-1)");
        headers[3].GetAttribute("style").Should().Be("right: var(--pin-right-3)");
    }

    [Fact]
    public async Task Pin_offset_vars_follow_header_position_when_a_column_is_hidden()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left),
                ("none", "C", PinSide.None))));

        // Act
        await SetColumnVisibilityAsync(renderedComponent, "A", visible: false);

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            var headers = renderedComponent.FindAll("th");
            headers[0].TextContent.Should().Contain("B");
            headers[0].GetAttribute("style").Should().Be("left: var(--pin-left-0)");

            AssertPinVarsMatchHeaderPositions(renderedComponent);
        });
    }

    [Fact]
    public async Task Pin_offset_vars_follow_header_position_after_a_drag_reorder()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left))));

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[0].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            var headers = renderedComponent.FindAll("th");
            headers[0].TextContent.Should().Contain("B");
            headers[0].GetAttribute("style").Should().Be("left: var(--pin-left-0)");
            headers[1].TextContent.Should().Contain("A");
            headers[1].GetAttribute("style").Should().Be("left: var(--pin-left-1)");
        });
    }

    [Fact]
    public void Changing_a_columns_pin_side_after_the_first_render_regroups_it()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("a", "A", PinSide.None),
                ("b", "B", PinSide.None))));

        renderedComponent.FindAll("th")[0].TextContent.Should().Contain("A");

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.Columns, PinnedColumns(
                ("a", "A", PinSide.None),
                ("b", "B", PinSide.Left))));

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            var headers = renderedComponent.FindAll("th");
            headers[0].TextContent.Should().Contain("B");
            headers[0].GetAttribute("style").Should().Contain("left: var(--pin-left-0)");
            headers[1].TextContent.Should().Contain("A");
        });
    }

    [Fact]
    public void Pinned_offsets_are_not_recomputed_by_a_render_that_leaves_the_headers_alone()
    {
        // Arrange
        var module = _testContext.JSInterop.SetupModule(TableModulePath);
        var attachResult = module.SetupModule(invocation => invocation.Identifier == "attach");
        attachResult.SetupVoid("updatePinnedOffsets").SetVoidResult();

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, PinnedColumns(
                ("left", "Left", PinSide.Left),
                ("none", "None", PinSide.None))));

        var callsAfterFirstRender = UpdatePinnedOffsetsCallCount(attachResult);
        callsAfterFirstRender.Should().BePositive();

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(2, "Other")])));

        // Assert
        UpdatePinnedOffsetsCallCount(attachResult).Should().Be(callsAfterFirstRender);
    }

    [Fact]
    public async Task Pinned_offsets_are_recomputed_after_a_column_width_is_committed()
    {
        // Arrange — the same settled table
        var module = _testContext.JSInterop.SetupModule(TableModulePath);
        var attachResult = module.SetupModule(invocation => invocation.Identifier == "attach");
        attachResult.SetupVoid("updatePinnedOffsets").SetVoidResult();

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, PinnedColumns(
                ("left", "Left", PinSide.Left),
                ("none", "None", PinSide.None))));

        var callsBeforeCommit = UpdatePinnedOffsetsCallCount(attachResult);

        // Act
        await renderedComponent.InvokeAsync(() =>
            renderedComponent.Instance.SetColumnWidthsAsync([new() { ColumnId = "left", Value = 250d }], null));

        // Assert
        renderedComponent.WaitForAssertion(() =>
            UpdatePinnedOffsetsCallCount(attachResult).Should().BeGreaterThan(callsBeforeCommit));
    }

    [Fact]
    public async Task Pinned_offsets_are_recomputed_when_a_column_is_hidden()
    {
        // Arrange — the same settled table
        var module = _testContext.JSInterop.SetupModule(TableModulePath);
        var attachResult = module.SetupModule(invocation => invocation.Identifier == "attach");
        attachResult.SetupVoid("updatePinnedOffsets").SetVoidResult();

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, PinnedColumns(
                ("left", "Left", PinSide.Left),
                ("none", "None", PinSide.None))));

        var callsBeforeHide = UpdatePinnedOffsetsCallCount(attachResult);

        // Act
        await SetColumnVisibilityAsync(renderedComponent, "Left", visible: false);

        // Assert
        renderedComponent.WaitForAssertion(() =>
            UpdatePinnedOffsetsCallCount(attachResult).Should().BeGreaterThan(callsBeforeHide));
    }

    [Fact]
    public void Left_pinned_column_has_pinned_col_class_on_cells()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Left))));

        // Assert
        renderedComponent.Find("td").ClassList.Should().Contain("pinned-column");
    }

    [Fact]
    public void Right_pinned_column_has_pin_right_style_on_cells()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Columns, PinnedColumns(("col", "Col", PinSide.Right))));

        // Assert
        renderedComponent.Find("td").GetAttribute("style").Should()
            .Contain("right: var(--pin-right-0)");
    }

    [Fact]
    public void Columns_render_in_order_left_then_unpinned_then_right_regardless_of_registration_order()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("right", "Right", PinSide.Right),
                ("none", "None", PinSide.None),
                ("left", "Left", PinSide.Left))));

        // Assert
        var headers = renderedComponent.FindAll("th");
        headers[0].GetAttribute("style").Should().Contain("left: var(--pin-left-");
        headers[1].ClassList.Should().NotContain("pinned-column");
        headers[2].GetAttribute("style").Should().Contain("right: var(--pin-right-");
    }

    [Fact]
    public async Task Dragging_column_within_same_pin_group_reorders_columns()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left))));

        // Act
        await renderedComponent.FindAll("th")[1].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[0].DragEnterAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[0].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            var reordered = renderedComponent.FindAll("th");
            reordered[0].TextContent.Should().Contain("B");
            reordered[1].TextContent.Should().Contain("A");
        });
    }

    [Fact]
    public async Task Dragging_column_across_pin_groups_does_not_change_order()
    {
        // Arrange — the second left pin is what lets this fail: with only one, the drop would reinsert it
        // in front of the unpinned column, the position it already holds, guard or no guard.
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left),
                ("none", "C", PinSide.None))));

        // Act: index 2 is the unpinned column — the gap zone at index 1 still belongs to the left group, so
        // dropping there would prove nothing
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragEnterAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[2].DropAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            var reordered = renderedComponent.FindAll("th");
            reordered[0].TextContent.Should().Contain("A");
            reordered[1].TextContent.Should().Contain("B");
            reordered[2].TextContent.Should().Contain("C");
        });
    }

    [Fact]
    public async Task Drop_indicator_shown_when_dragging_within_same_pin_group()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("a", "A", PinSide.None),
                ("b", "B", PinSide.None))));

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(() =>
            renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start"));
    }

    [Fact]
    public async Task Drop_indicator_not_shown_when_dragging_across_pin_groups()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left", "Left", PinSide.Left),
                ("none", "None", PinSide.None))));

        // Act
        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());

        // Assert
        renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().BeEmpty();
    }

    [Fact]
    public async Task Dragging_from_a_gap_zone_into_a_dead_zone_clears_the_indicator()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, PinnedColumns(
                ("left-a", "A", PinSide.Left),
                ("left-b", "B", PinSide.Left),
                ("none", "None", PinSide.None))));

        await renderedComponent.FindAll("th")[0].DragStartAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragEnterAsync(new DragEventArgs());
        renderedComponent.FindAll("th")[1].ClassList.Should().Contain("gap-indicator-start");

        // Act
        await renderedComponent.FindAll("th .gap-zone-start")[2].DragEnterAsync(new DragEventArgs());
        await renderedComponent.FindAll("th .gap-zone-start")[1].DragLeaveAsync(new DragEventArgs());

        // Assert
        renderedComponent.WaitForAssertion(()
            => renderedComponent.FindAll("th.gap-indicator-start, th.gap-indicator-end").Should().BeEmpty());
    }

    private static void AssertPinVarsMatchHeaderPositions(
        IRenderedComponent<AdvancedTable<TableTestItem>> rendered)
    {
        var headers = rendered.FindAll("th");

        for (var index = 0; index < headers.Count; index++)
        {
            var style = headers[index].GetAttribute("style");

            if (string.IsNullOrEmpty(style))
                continue;

            style.Should().BeOneOf($"left: var(--pin-left-{index})", $"right: var(--pin-right-{index})");
        }
    }

    private static int UpdatePinnedOffsetsCallCount(BunitJSModuleInterop attachResult)
        => attachResult.Invocations["updatePinnedOffsets"].Count;

    private static RenderFragment PinnedColumns(params (string Id, string Title, PinSide PinSide)[] columns)
        => builder =>
        {
            var seq = 0;
            foreach (var (id, title, pinSide) in columns)
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(seq++);
                {
                    builder.AddComponentParameter(seq++, nameof(AdvancedTableColumn<>.Id), id);
                    builder.AddComponentParameter(seq++, nameof(AdvancedTableColumn<>.Title), title);
                    builder.AddComponentParameter(
                        seq++, nameof(AdvancedTableColumn<>.CellContent),
                        (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                    builder.AddComponentParameter(seq++, nameof(AdvancedTableColumn<>.PinSide), pinSide);
                }
                builder.CloseComponent();
            }
        };
}
