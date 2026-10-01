using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public async Task Dragging_one_of_several_selected_rows_carries_the_whole_selection_without_mutating_it()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var item3 = new TableTestItem(3, "Three");
        List<TableTestItem>? selectionChange = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2, item3])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(item2);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item2));

        // Assert: the frozen payload is the whole selection in selection order, no mutation
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1, item2, item3));
        selectionChange.Should().BeNull();
    }

    [Fact]
    public async Task Dragging_an_unselected_row_single_selects_it_like_a_click_and_carries_just_that_row()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var item3 = new TableTestItem(3, "Three");
        List<TableTestItem>? selectionChange = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(item3);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item3));

        // Assert: selection collapses to the dragged row and the payload is just that row
        rendered.WaitForAssertion(() => selectionChange.Should().Equal(item3));
        payload.Items.Should().Equal(item3);
    }

    [Fact]
    public async Task Dragging_an_unselected_row_with_row_click_selection_disabled_does_not_mutate_the_selection()
    {
        // Arrange
        var item = new TableTestItem(1, "One");
        List<TableTestItem>? selectionChange = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(item);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item));

        // Assert: the display-only channel must not become a back-door selection channel
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item));
        selectionChange.Should().BeNull();
    }

    [Fact]
    public async Task Dragging_an_unselected_vetoed_row_does_not_mutate_the_selection()
    {
        // Arrange
        var item = new TableTestItem(1, "One");
        List<TableTestItem>? selectionChange = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, _ => false)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(item);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item));

        // Assert: a vetoed row cannot be selected by a drag any more than by a click
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item));
        selectionChange.Should().BeNull();
    }

    [Fact]
    public async Task Dragging_a_key_equal_but_different_instance_counts_as_selected_and_the_payload_keeps_the_selection_instances()
    {
        // Arrange: selection holds one instance; the provider renders a fresh instance with the same id
        var selectedInstance = new ReferenceItem(1, "Selected instance");
        var renderedInstance = new ReferenceItem(1, "Rendered instance");
        List<ReferenceItem>? selectionChange = null;

        var rendered = _testContext.Render<AdvancedTable<ReferenceItem>>(b => b
            .Add(p => p.ItemsProvider, new ReferenceItemsProvider([renderedInstance, new(2, "B")]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.Id)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [selectedInstance])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<ReferenceItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(renderedInstance);

        // Act: drag the rendered instance, which is key-equal to the selected one
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(renderedInstance));

        // Assert: counted as selected (no collapse); payload carries the selection's instance, not the dragged one
        rendered.WaitForAssertion(() => payload.Items.Should().ContainSingle().Which.Should().BeSameAs(selectedInstance));
        selectionChange.Should().BeNull();
    }

    [Fact]
    public async Task The_drag_payload_is_frozen_at_drag_start_and_survives_a_later_selection_change()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2]));

        var payload = PayloadOf(item1);

        // Act: freeze the payload at drag start, then mutate the selection out from under it
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1, item2));

        rendered.Render(b => b
            .Add(p => p.SelectedItems, [item1]));

        // Assert: the drop side still reads the snapshot grabbed at drag start
        payload.Items.Should().Equal(item1, item2);
    }

    [Fact]
    public void The_drop_side_set_reads_as_the_row_item_before_any_drag()
    {
        // Arrange
        var item = new TableTestItem(1, "One");

        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.RowsDraggable, true));

        // Act
        var payload = (IDraggableRowSet<TableTestItem>)_fakeDragInteraction.LastAttached!;

        // Assert: never empty — degrades to the single dragged row
        payload.Items.Should().Equal(item);
    }

    [Fact]
    public void Draggable_rows_attach_with_the_table_row_drag_ghost_to_avoid_row_collapse()
    {
        // Arrange & Act
        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        // Assert: the row supplies the table-row ghost, keeping a dragged <tr> at its rendered width
        _fakeDragInteraction.LastDragGhost.Should().BeAssignableTo<ITableRowDragGhost>();
    }

    [Fact]
    public async Task A_drag_payload_provider_narrowing_the_selection_carries_only_the_rows_it_returned()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var item3 = new TableTestItem(3, "Three");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2, item3])
            .Add(p => p.DragPayloadProvider,
                new TestDragPayloadProvider((_, payload) => [.. payload.Where(i => i.TestKey != 2)])));

        var payload = PayloadOf(item2);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item2));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1, item3));
    }

    [Fact]
    public async Task A_drag_payload_provider_receives_the_dragged_row_and_the_payload_the_table_resolved()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        TableTestItem? draggedRow = null;
        IReadOnlyList<TableTestItem>? resolvedPayload = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.DragPayloadProvider, new TestDragPayloadProvider((dragged, payload) =>
            {
                draggedRow = dragged;
                resolvedPayload = payload;

                return payload;
            })));

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item2));

        // Assert
        rendered.WaitForAssertion(() => draggedRow.Should().BeSameAs(item2));
        resolvedPayload.Should().Equal(item1, item2);
    }

    [Fact]
    public async Task A_drag_payload_provider_is_consulted_on_an_unselected_row_too_and_the_single_select_still_stands()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        List<TableTestItem>? selectionChange = null;
        IReadOnlyList<TableTestItem>? resolvedPayload = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l))
            .Add(p => p.DragPayloadProvider, new TestDragPayloadProvider((_, payload) =>
            {
                resolvedPayload = payload;

                return [];
            })));

        var payload = PayloadOf(item2);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item2));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().BeEmpty());
        resolvedPayload.Should().Equal(item2);
        selectionChange.Should().Equal(item2);
    }

    [Fact]
    public async Task A_drag_payload_provider_returning_nothing_leaves_the_drop_side_set_empty_on_this_and_the_next_drag()
    {
        // Arrange: the provider answers with the whole selection first, with nothing on the second drag
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        IReadOnlyList<TableTestItem> providerResult = [item1, item2];

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.DragPayloadProvider, new TestDragPayloadProvider((_, _) => providerResult)));

        var payload = PayloadOf(item1);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1, item2));

        providerResult = [];
        await _fakeDragInteraction.RaiseDragStartAsync((IDraggable)payload);

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().BeEmpty());
    }

    [Fact]
    public async Task A_throwing_drag_payload_provider_carries_nothing_and_logs_an_error()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.DragPayloadProvider,
                new TestDragPayloadProvider((_, _) => throw new InvalidOperationException("Provider failed."))));

        var payload = PayloadOf(item1);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().BeEmpty());
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    [Fact]
    public async Task A_drag_payload_provider_may_answer_with_rows_that_were_never_in_the_resolved_payload()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var stranger = new TableTestItem(99, "Not a row of this table");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.DragPayloadProvider,
                new TestDragPayloadProvider((_, payload) => [stranger, .. payload.Reverse()])));

        var payload = PayloadOf(item1);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(stranger, item2, item1));
    }

    // A drop policy decides at DragStart. It used to read the at-rest [Item] here, a new instance per read, which
    // defeated any cache keyed on the payload.
    [Fact]
    public async Task A_drag_start_listener_already_reads_the_payload_of_the_first_drag()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var item3 = new TableTestItem(3, "Three");

        // Subscribed before the rows are, as a diagram's dropzones are: the rows' own handlers run after it.
        IReadOnlyList<TableTestItem>? payloadAtDragStart = null;
        _fakeDragInteraction.DragStart += (_, args)
            => payloadAtDragStart = ((IDraggableRowSet<TableTestItem>)args.Draggable).Items;

        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2, item3]));

        var draggable = Draggable(item2);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(draggable);

        // Assert
        payloadAtDragStart.Should().Equal(item1, item2, item3);
    }

    [Fact]
    public async Task A_drag_start_listener_reads_the_payload_of_a_later_drag_not_the_previous_ones()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");

        // Subscribed before the rows are, as a diagram's dropzones are: the rows' own handlers run after it.
        IReadOnlyList<TableTestItem>? payloadAtDragStart = null;
        _fakeDragInteraction.DragStart += (_, args)
            => payloadAtDragStart = ((IDraggableRowSet<TableTestItem>)args.Draggable).Items;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2]));

        var draggable = Draggable(item1);
        await _fakeDragInteraction.RaiseDragStartAsync(draggable);

        rendered.Render(b => b
            .Add(p => p.SelectedItems, [item1]));

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(draggable);

        // Assert
        payloadAtDragStart.Should().Equal(item1);
    }

    // Identify a specific row's draggable. Called before any drag, so every row's payload is still its
    // at-rest single item ([Item]) — that is the stable per-row identity to match on.
    private IDraggable Draggable<TItem>(TItem item)
        where TItem : class
        => _fakeDragInteraction.Attached.Single(d =>
            ((IDraggableRowSet<TItem>)d).Items is [var only] && ReferenceEquals(only, item));

    private IDraggableRowSet<TItem> PayloadOf<TItem>(TItem item)
        where TItem : class
        => (IDraggableRowSet<TItem>)Draggable(item);
}
