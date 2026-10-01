using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public async Task Dragging_a_selected_row_through_the_public_table_carries_the_whole_selection()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item1, item2])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2]));

        var payload = PayloadOf(item1);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1, item2));
    }

    [Fact]
    public async Task Dragging_an_unselected_row_through_the_public_table_single_selects_it_and_carries_that_row()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        List<TableTestItem>? selectionChange = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item1, item2])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectionChange = l)));

        var payload = PayloadOf(item2);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item2));

        // Assert
        rendered.WaitForAssertion(() => selectionChange.Should().Equal(item2));
        payload.Items.Should().Equal(item2);
    }

    [Fact]
    public void Draggable_rows_attach_with_the_table_row_drag_ghost_to_avoid_row_collapse()
    {
        // Arrange & Act
        _ = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [new(1, "Value")])
            .Add(p => p.RowsDraggable, true));

        // Assert: the row supplies the table-row ghost, keeping a dragged <tr> at its rendered width
        _fakeDragInteraction.LastDragGhost.Should().BeAssignableTo<ITableRowDragGhost>();
    }

    [Fact]
    public async Task A_drag_payload_provider_set_on_the_public_table_reaches_the_payload()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item1, item2])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.DragPayloadProvider, new TestDragPayloadProvider((dragged, _) => [dragged])));

        var payload = PayloadOf(item1);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(item1));

        // Assert: an unforwarded parameter would carry the whole selection
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(item1));
    }

    [Fact]
    public async Task A_visible_selection_provider_carries_only_the_selected_rows_the_filter_leaves()
    {
        // Arrange — of the two selected rows only one passes the filter, and the provider scopes the drag to the
        // consumer's own mirror of the visible selection
        var items = BuildVisibleSelectionItems();
        HashSet<TableTestItem> visibleSelection = [];

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowsDraggable, true)
            .Add(p => p.SelectedItems, [items[0], items[3]])
            .Add(p => p.FilterState, ValueContainsFilter("Match"))
            .Add(p => p.VisibleSelectionChanged, list => visibleSelection = [.. list])
            .Add(p => p.DragPayloadProvider,
                new TestDragPayloadProvider((_, payload) => [.. payload.Where(visibleSelection.Contains)]))
            .AddColumns());

        rendered.WaitForAssertion(() => visibleSelection.Should().Equal(items[0]));

        var payload = PayloadOf(items[0]);

        // Act
        await _fakeDragInteraction.RaiseDragStartAsync(Draggable(items[0]));

        // Assert
        rendered.WaitForAssertion(() => payload.Items.Should().Equal(items[0]));
        rendered.Instance.SelectedItems.Should().BeEquivalentTo([items[0], items[3]]);
    }

    // Identify a specific row's draggable. Called before any drag, so every row's payload is still its
    // at-rest single item ([Item]) — that is the stable per-row identity to match on.
    private IDraggable Draggable(TableTestItem item)
        => _fakeDragInteraction.Attached.Single(d =>
            ((IDraggableRowSet<TableTestItem>)d).Items is [var only] && ReferenceEquals(only, item));

    private IDraggableRowSet<TableTestItem> PayloadOf(TableTestItem item)
        => (IDraggableRowSet<TableTestItem>)Draggable(item);
}
