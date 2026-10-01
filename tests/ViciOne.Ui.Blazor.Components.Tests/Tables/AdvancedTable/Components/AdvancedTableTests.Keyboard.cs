using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

// The keyboard gestures are raised by the module, which addresses a row by the sequence number its cells
// carry, so these tests call the very entry points it calls and read the numbers off the rendered cells the
// way it reads them. Movement itself has no C# side at all and is covered by the Playwright tier.
public sealed partial class AdvancedTableTests
{
    [Fact]
    public async Task Space_gesture_selects_the_row_it_names()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1);

        // Assert
        received.Should().BeEquivalentTo([items[1]]);
    }

    [Fact]
    public async Task Space_gesture_replaces_the_whole_selection()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 0, ctrlKey: true);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1, ctrlKey: true);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 2);

        // Assert
        received.Should().BeEquivalentTo([items[2]]);
    }

    [Fact]
    public async Task Ctrl_space_gesture_adds_the_row_to_the_selection()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 0, ctrlKey: true);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 2, ctrlKey: true);

        // Assert
        received.Should().BeEquivalentTo([items[0], items[2]]);
    }

    [Fact]
    public async Task Ctrl_space_gesture_takes_a_selected_row_out_again()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1, ctrlKey: true);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1, ctrlKey: true);

        // Assert
        received.Should().BeEmpty();
    }

    [Fact]
    public async Task Shift_space_gesture_selects_the_range_from_the_anchor()
    {
        // Arrange
        var items = BuildItems(5);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act — a plain gesture sets the anchor, the shifted one measures from it
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 3, shiftKey: true);

        // Assert
        received.Should().BeEquivalentTo([items[1], items[2], items[3]]);
    }

    [Fact]
    public async Task Shift_space_gesture_reversing_direction_shrinks_the_range()
    {
        // Arrange
        var items = BuildItems(5);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act — the anchor stays put, so reversing past it measures the other way rather than adding on
        await RaiseSelectionGestureAsync(rendered, rowPosition: 2);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 4, shiftKey: true);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1, shiftKey: true);

        // Assert
        received.Should().BeEquivalentTo([items[1], items[2]]);
    }

    [Fact]
    public async Task Space_gesture_on_a_vetoed_row_does_not_fire_the_callback()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, request => request.Item != items[1])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .Add(p => p.Columns, KeyboardColumns()));

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1);

        // Assert — the veto is the selection's, not movement's: the row stays reachable and stays unselected
        received.Should().BeNull();
    }

    [Fact]
    public async Task Space_gesture_in_single_mode_ignores_both_modifiers()
    {
        // Arrange
        var items = BuildItems(4);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Single, l => received = l);

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 0);
        await RaiseSelectionGestureAsync(rendered, rowPosition: 2, ctrlKey: true, shiftKey: true);

        // Assert — the same replacement a plain gesture makes, exactly as the click branches behave
        received.Should().BeEquivalentTo([items[2]]);
    }

    [Fact]
    public async Task Space_gesture_does_nothing_while_row_click_selection_is_off()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .Add(p => p.Columns, KeyboardColumns()));

        // Act
        await RaiseSelectionGestureAsync(rendered, rowPosition: 1);

        // Assert
        received.Should().BeNull();
    }

    [Fact]
    public async Task Cell_activation_still_fires_while_row_click_selection_is_off()
    {
        // Arrange
        var items = BuildItems(3);
        TableTestItem? activated = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.Columns, KeyboardColumns(activatedColumnId: "wired",
                cellActivated: EventCallback.Factory.Create<TableTestItem>(this, item => activated = item))));

        // Act
        await RaiseActivationAsync(rendered, rowPosition: 2, columnId: "wired");

        // Assert — selection is gated, activation is not
        activated.Should().Be(items[2]);
    }

    [Fact]
    public async Task Cell_activation_fires_on_the_column_the_cell_belongs_to()
    {
        // Arrange
        var items = BuildItems(2);
        TableTestItem? activated = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, KeyboardColumns(activatedColumnId: "wired",
                cellActivated: EventCallback.Factory.Create<TableTestItem>(this, item => activated = item))));

        // Act
        await RaiseActivationAsync(rendered, rowPosition: 1, columnId: "wired");

        // Assert
        activated.Should().Be(items[1]);
    }

    [Fact]
    public async Task Cell_activation_on_a_column_without_a_handler_does_nothing()
    {
        // Arrange
        var items = BuildItems(2);
        TableTestItem? activated = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, KeyboardColumns(activatedColumnId: "wired",
                cellActivated: EventCallback.Factory.Create<TableTestItem>(this, item => activated = item))));

        // Act — the same row, through the column that was left unwired
        await RaiseActivationAsync(rendered, rowPosition: 1, columnId: "unwired");

        // Assert — silent, and the wired column's handler is not borrowed for it
        activated.Should().BeNull();
    }

    [Fact]
    public async Task Cell_activation_for_an_unknown_column_does_nothing()
    {
        // Arrange
        var items = BuildItems(2);
        TableTestItem? activated = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, KeyboardColumns(activatedColumnId: "wired",
                cellActivated: EventCallback.Factory.Create<TableTestItem>(this, item => activated = item))));

        // Act
        await RaiseActivationAsync(rendered, rowPosition: 0, columnId: "gone");

        // Assert
        activated.Should().BeNull();
    }

    [Fact]
    public async Task Cell_activation_enters_the_cell_after_the_render_the_handler_caused()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("enterFocusedCell").SetVoidResult();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var items = BuildItems(2);
        var handlerRan = false;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, KeyboardColumns(activatedColumnId: "wired",
                cellActivated: EventCallback.Factory.Create<TableTestItem>(this, () => handlerRan = true))));

        var callsAfterRender = attachResult.Invocations["enterFocusedCell"].Count;

        // Act
        await RaiseActivationAsync(rendered, rowPosition: 0, columnId: "wired");

        // Assert — an editor a handler swaps in is not there to be focused until its render has landed
        handlerRan.Should().BeTrue();
        attachResult.Invocations["enterFocusedCell"].Count.Should().Be(callsAfterRender);

        rendered.Render();

        attachResult.Invocations["enterFocusedCell"].Count.Should().BeGreaterThan(callsAfterRender);
    }

    [Fact]
    public async Task Cell_activation_on_a_column_without_a_handler_enters_what_the_cell_already_holds()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("enterFocusedCell").SetVoidResult();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(2)))
            .Add(p => p.Columns, KeyboardColumns()));

        var callsAfterRender = attachResult.Invocations["enterFocusedCell"].Count;

        // Act
        await RaiseActivationAsync(rendered, rowPosition: 0, columnId: "unwired");

        // Assert — nothing is going to re-render, so waiting for a render would wait forever
        attachResult.Invocations["enterFocusedCell"].Count.Should().BeGreaterThan(callsAfterRender);
    }

    [Fact]
    public async Task A_row_sequence_the_table_never_handed_out_resolves_to_nothing()
    {
        // Arrange
        var items = BuildItems(3);
        List<TableTestItem>? received = null;

        var rendered = RenderKeyboardTable(items, SelectionMode.Multiple, l => received = l);

        // Act — the number a cell left over from an earlier pass would carry
        await rendered.InvokeAsync(() => rendered.Instance.SelectRowAsync(int.MaxValue, ctrlKey: false, shiftKey: false));

        // Assert — nothing at all, rather than whichever item now occupies that row
        received.Should().BeNull();
    }

    [Fact]
    public void The_scroll_container_is_no_tab_stop_and_carries_the_row_click_flag()
    {
        // Arrange & Act
        var rendered = RenderKeyboardTable(BuildItems(1), SelectionMode.Multiple, _ => { });

        // Assert — the cells hold the stop; a stop on the container as well would be a second one
        var container = rendered.Find(".inner-table-container");
        container.HasAttribute("tabindex").Should().BeFalse();
        container.HasAttribute("data-row-click-selection").Should().BeTrue();
    }

    [Fact]
    public void The_row_click_flag_is_absent_while_row_click_selection_is_off()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(1)))
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.Columns, KeyboardColumns()));

        // Assert — the module reads the flag to drop a gesture without paying for the round trip
        rendered.Find(".inner-table-container").HasAttribute("data-row-click-selection").Should().BeFalse();
    }

    // The tab order itself needs a browser and lives in the Playwright tier, but its load-bearing half is a
    // rendering fact and belongs where CI can see it. The value .NET renders never varies, so Blazor's diff
    // never writes the attribute and the single cell JavaScript promotes to "0" survives every re-render —
    // which is the whole reason a re-render cannot cost the table its tab stop.
    [Fact]
    public void Every_cell_renders_the_same_tabindex_whatever_the_render()
    {
        // Arrange
        var rendered = RenderKeyboardTable(BuildItems(3), SelectionMode.Multiple, _ => { });

        // Act — a second render, of the kind a selection or a resize causes
        rendered.Render();

        // Assert
        rendered.FindAll("tbody td").Should().NotBeEmpty()
            .And.AllSatisfy(cell => cell.GetAttribute("tabindex").Should().Be("-1"));

        rendered.FindAll("thead th").Should().NotBeEmpty()
            .And.AllSatisfy(header => header.GetAttribute("tabindex").Should().Be("-1"));

        // The row is not a position, and neither is the content of a header cell
        rendered.FindAll("tbody tr").Should().AllSatisfy(row => row.HasAttribute("tabindex").Should().BeFalse());
        rendered.FindAll("thead .header-cell").Should().NotBeEmpty()
            .And.AllSatisfy(content => content.HasAttribute("tabindex").Should().BeFalse());
    }

    // The other half of the tab order .NET owns. The boundaries are what Tab actually lands on, so a render
    // that stopped emitting them, or emitted them unfocusable, would take the table out of the tab order
    // altogether — and every keyboard test above would still pass, because they drive the cells directly.
    [Fact]
    public void The_table_is_bounded_by_two_focusable_elements_outside_the_scroll_container()
    {
        // Arrange
        var rendered = RenderKeyboardTable(BuildItems(2), SelectionMode.Multiple, _ => { });

        // Act — a second render, of the kind a selection or a resize causes
        rendered.Render();

        // Assert
        var boundaries = rendered.FindAll(".content-container > .tab-boundary");
        boundaries.Should().HaveCount(2)
            .And.AllSatisfy(boundary => boundary.GetAttribute("tabindex").Should().Be("0"));

        // One either side of the scroll container, which is what puts them at the ends of the tab order
        var contentChildren = rendered.Find(".content-container").Children;
        contentChildren.Should().HaveCountGreaterThanOrEqualTo(3);
        contentChildren[0].ClassName.Should().Be("tab-boundary");
        contentChildren[^1].ClassName.Should().Be("tab-boundary");
    }

    // The library owns the tab order by bounding it, never by writing on content it did not render — so a
    // select checkbox keeps whatever the CheckBox component gives it and is reached by entering its cell.
    [Fact]
    public void The_table_writes_no_tabindex_onto_the_content_of_a_cell()
    {
        // Arrange & Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(2)))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.Columns, SelectAndKeyboardColumns()));

        // Assert
        rendered.FindAll("tbody td input[type=\"checkbox\"]").Should().NotBeEmpty()
            .And.AllSatisfy(checkBox => checkBox.HasAttribute("tabindex").Should().BeFalse());
    }

    [Fact]
    public async Task The_no_data_placeholder_holds_the_tab_stop_when_every_column_is_hidden()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(3)))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        // Act
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);
        await SetColumnVisibilityAsync(rendered, "colB", visible: false);

        // Assert — rows with no cells left in them would leave the table with nothing Tab can reach
        rendered.FindAll("tbody tr.no-data td").Should().ContainSingle()
            .Which.GetAttribute("tabindex").Should().Be("-1");
    }

    [Fact]
    public void Every_body_cell_names_its_column_its_row_sequence_and_its_row_index()
    {
        // Arrange & Act
        var rendered = RenderKeyboardTable(BuildItems(2), SelectionMode.Multiple, _ => { });

        // Assert
        var rows = rendered.FindAll("tbody tr");
        rows.Should().HaveCount(2);

        for (var rowPosition = 0; rowPosition < rows.Count; rowPosition++)
        {
            var cells = rows[rowPosition].QuerySelectorAll("td");
            cells.Should().HaveCount(2);

            cells.Select(cell => cell.GetAttribute("data-column-id")).Should().Equal("wired", "unwired");

            // One sequence number per row, on every cell of it, and the absolute position of the row
            cells.Select(cell => cell.GetAttribute("data-row-sequence")).Distinct().Should().ContainSingle();
            cells.Select(cell => cell.GetAttribute("data-row-index")).Should()
                .AllBe(rowPosition.ToString(CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public async Task Row_indexes_are_absolute_once_the_loaded_window_has_moved()
    {
        // Arrange
        var items = BuildItems(60);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(items))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.ItemIdSelector, item => item.TestKey)
            .Add(p => p.Columns, KeyboardColumns()));

        // Act — driving Virtualize's own provider leaves the rows of the first window rendered and appends
        // the new one after them, so the moved window starts at the fiftieth rendered row
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);

        // Assert — a position within the window would read zero here; what movement steps by is the position
        // in the filtered, sorted set
        var movedWindowCell = rendered.FindAll("tbody tr")[50].QuerySelector("td");
        movedWindowCell.Should().NotBeNull();
        movedWindowCell.GetAttribute("data-row-index").Should().Be("50");

        // And a rendered row the loaded window no longer holds names no position at all, so the keyboard
        // cannot be sent to a row that belongs to someone else
        var evictedCell = rendered.FindAll("tbody tr")[0].QuerySelector("td");
        evictedCell.Should().NotBeNull();
        evictedCell.GetAttribute("data-row-index").Should().Be("-1");
    }

    [Fact]
    public async Task A_row_sequence_resolves_to_the_item_its_row_shows_after_the_window_moved()
    {
        // Arrange
        var items = BuildItems(60);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(items))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, item => item.TestKey)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .Add(p => p.Columns, KeyboardColumns()));

        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);

        // Act — the third row of the moved window, which is rendered after the rows of the first one
        await RaiseSelectionGestureAsync(rendered, rowPosition: 52);

        // Assert
        received.Should().BeEquivalentTo([items[52]]);
    }

    [Fact]
    public async Task Applying_a_filter_resets_the_focused_cell()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(5)))
            .Add(p => p.Columns, FilterColumn("col")));

        // Act
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["resetFocusedCell"].Should().NotBeEmpty());
    }

    [Fact]
    public async Task Hiding_a_column_resets_the_focused_cell()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(BuildItems(5)))
            .Add(p => p.Columns, ChooserColumns("colA", "colB")));

        var callsAfterRender = attachResult.Invocations["resetFocusedCell"].Count;

        // Act — neither column carries a filter or a sorting, so this hide re-fetches nothing
        await SetColumnVisibilityAsync(rendered, "colA", visible: false);

        // Assert
        await rendered.WaitForAssertionAsync(() =>
            attachResult.Invocations["resetFocusedCell"].Count.Should().BeGreaterThan(callsAfterRender));
    }

    [Fact]
    public async Task Moving_the_loaded_window_leaves_the_focused_cell_alone()
    {
        // Arrange
        var attachResult = SetupTableModule();
        attachResult.SetupVoid("resetFocusedCell").SetVoidResult();

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(BuildItems(60)))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.ItemIdSelector, item => item.TestKey)
            .Add(p => p.Columns, KeyboardColumns()));

        var callsAfterRender = attachResult.Invocations["resetFocusedCell"].Count;

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);

        // Assert: the same rows scrolling past would make the marker vanish on every wheel-scroll
        attachResult.Invocations["resetFocusedCell"].Count.Should().Be(callsAfterRender);
    }

    private IRenderedComponent<AdvancedTable<TableTestItem>> RenderKeyboardTable(
        List<TableTestItem> items, SelectionMode selectionMode, Action<List<TableTestItem>> selectionChanged)
            => _testContext.Render<AdvancedTable<TableTestItem>>(b => b
                .Add(p => p.ItemsProvider, new TestItemsProvider(items))
                .Add(p => p.SelectionMode, selectionMode)
                .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create(this, selectionChanged))
                .Add(p => p.Columns, KeyboardColumns()));

    // The select column ahead of the plain ones, so a test can look at library-rendered content inside a cell.
    private static RenderFragment SelectAndKeyboardColumns()
        => builder =>
        {
            builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
            builder.CloseComponent();

            KeyboardColumns()(builder);
        };

    // Two columns, only the first of which can carry a handler, so the tests can check both that activation
    // reaches the right column and that a column left unwired stays silent.
    private static RenderFragment KeyboardColumns(string? activatedColumnId = null,
        EventCallback<TableTestItem> cellActivated = default)
            => builder =>
            {
                var sequence = 0;

                foreach (var columnId in new[] { "wired", "unwired" })
                {
                    builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence++);
                    {
                        builder.AddComponentParameter(sequence++, nameof(AdvancedTableColumn<>.Id), columnId);

                        if (columnId == activatedColumnId)
                        {
                            builder.AddComponentParameter(sequence++,
                                nameof(AdvancedTableColumn<>.CellActivated), cellActivated);
                        }
                    }
                    builder.CloseComponent();
                }
            };

    // Raises the gesture the module raises for the Space family, naming the row the way it names it: by the
    // sequence number the rendered cell carries, never by its position.
    private static async Task RaiseSelectionGestureAsync(
        IRenderedComponent<AdvancedTable<TableTestItem>> rendered, int rowPosition,
        bool ctrlKey = false, bool shiftKey = false)
    {
        var rowSequence = GetRowSequence(rendered, rowPosition);

        await rendered.InvokeAsync(() => rendered.Instance.SelectRowAsync(rowSequence, ctrlKey, shiftKey));
    }

    private static async Task RaiseActivationAsync(
        IRenderedComponent<AdvancedTable<TableTestItem>> rendered, int rowPosition, string columnId)
    {
        var rowSequence = GetRowSequence(rendered, rowPosition);

        await rendered.InvokeAsync(() => rendered.Instance.ActivateCellAsync(rowSequence, columnId));
    }

    private static int GetRowSequence(IRenderedComponent<AdvancedTable<TableTestItem>> rendered, int rowPosition)
    {
        var cell = rendered.FindAll("tbody tr")[rowPosition].QuerySelector("td");
        cell.Should().NotBeNull();

        var rowSequence = cell.GetAttribute("data-row-sequence");
        rowSequence.Should().NotBeNull();

        return int.Parse(rowSequence, CultureInfo.InvariantCulture);
    }
}
