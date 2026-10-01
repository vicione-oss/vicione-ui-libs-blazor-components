using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

// Blazor's Virtualization namespace declares its own ItemsProviderResult<T>, which collides with the table's.
using Virtualization = Microsoft.AspNetCore.Components.Web.Virtualization;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public async Task SelectionMode_single_clicking_row_selects_item_and_fires_callback()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item]);
    }

    [Fact]
    public async Task SelectionMode_single_clicking_another_row_replaces_selection()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item2]);
    }

    [Fact]
    public async Task SelectionMode_single_clicking_unselectable_row_does_not_fire_callback()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.ItemSelectionAllowed, _ => false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeNull();
    }

    [Fact]
    public async Task SelectionMode_multiple_plain_click_selects_item()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item]);
    }

    [Fact]
    public async Task SelectionMode_multiple_ctrl_click_adds_item_to_selection()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { CtrlKey = true });
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { CtrlKey = true });

        // Assert
        received.Should().BeEquivalentTo([item1, item2]);
    }

    [Fact]
    public async Task SelectionMode_multiple_ctrl_click_existing_item_removes_it()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs { CtrlKey = true });
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs { CtrlKey = true });

        // Assert
        received.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectionMode_multiple_ctrl_click_unselectable_item_does_not_add_it_or_fire_the_callback()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, _ => false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs { CtrlKey = true });

        // Assert: silent like the plain-click path, rather than announcing a selection that did not change
        received.Should().BeNull();
    }

    [Fact]
    public async Task SelectionMode_multiple_shift_click_selects_range()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var item3 = new TableTestItem(3, "Value 3");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[2].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().BeEquivalentTo([item1, item2, item3]);
    }

    [Fact]
    public async Task SelectionMode_multiple_shift_click_upward_walks_from_anchor_toward_target()
    {
        // Arrange: a cap of three selectable items makes the surviving membership depend on traversal order.
        // Walking from the anchor (bottom) upward keeps item5, item4, item3; walking bottom-up document order
        // would instead keep item5 plus item1, item2.
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var item3 = new TableTestItem(3, "Value 3");
        var item4 = new TableTestItem(4, "Value 4");
        var item5 = new TableTestItem(5, "Value 5");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3, item4, item5]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, req => req.CurrentSelection.Count < 3)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act: anchor on the bottom row, then shift-click the top row
        await rendered.FindAll("tbody tr")[4].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().BeEquivalentTo([item5, item4, item3]);
    }

    [Fact]
    public async Task SelectionMode_multiple_shift_click_skips_unselectable_items_in_range()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2"); // unselectable
        var item3 = new TableTestItem(3, "Value 3");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, req => req.Item.TestKey != 2)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[2].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().BeEquivalentTo([item1, item3]);
    }

    [Fact]
    public async Task SelectionMode_multiple_consecutive_shift_clicks_use_first_anchor()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var item3 = new TableTestItem(3, "Value 3");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs());                          // anchor = item2
        await rendered.FindAll("tbody tr")[2].ClickAsync(new MouseEventArgs { ShiftKey = true });        // range: item2..item3
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });        // range: item1..item2 (anchor still item2)

        // Assert
        received.Should().BeEquivalentTo([item1, item2]);
    }

    [Fact]
    public async Task Shift_click_after_an_items_provider_swap_measures_from_the_anchor_rows_new_position()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var item3 = new TableTestItem(3, "Value 3");
        var item4 = new TableTestItem(4, "Value 4");
        var item5 = new TableTestItem(5, "Value 5");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2, item3, item4, item5]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        await rendered.FindAll("tbody tr")[4].ClickAsync(new MouseEventArgs());

        // Act
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item5, item4, item3, item2, item1])));

        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().BeEquivalentTo([item5, item4]);
    }

    [Fact]
    public async Task Shift_click_with_both_ends_loaded_makes_no_additional_provider_call()
    {
        // Arrange
        var items = BuildItems(5);
        var provider = new RangedItemsProvider(items);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SelectionMode, SelectionMode.Multiple));

        var callsAfterRender = provider.CallCount;

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[4].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        provider.CallCount.Should().Be(callsAfterRender);
    }

    [Fact]
    public async Task Shift_click_with_the_anchor_outside_the_loaded_window_selects_the_whole_span()
    {
        // Arrange
        var items = BuildItems(60);
        var provider = new RangedItemsProvider(items);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.TestKey)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);
        await rendered.FindAll("tbody tr")[50].ClickAsync(new MouseEventArgs());  // absolute index 50

        await MoveLoadedWindowAsync(rendered, startIndex: 0, count: 10);
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });  // absolute index 0

        // Assert
        received.Should().NotBeNull();
        received.Should().HaveCount(51);
        received.Select(x => x.TestKey).Should().BeEquivalentTo(Enumerable.Range(0, 51).Reverse());
    }

    [Fact]
    public async Task Shift_click_reaching_outside_the_loaded_window_asks_the_provider_for_the_spanned_range()
    {
        // Arrange
        var items = BuildItems(60);
        var provider = new RangedItemsProvider(items);

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.TestKey));

        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);
        await rendered.FindAll("tbody tr")[50].ClickAsync(new MouseEventArgs());  // anchor at absolute index 50

        await MoveLoadedWindowAsync(rendered, startIndex: 0, count: 10);

        var callsBeforeRange = provider.CallCount;

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        provider.CallCount.Should().Be(callsBeforeRange + 1);

        var rangeContext = provider.ReceivedContexts[^1];
        rangeContext.ItemRange.Should().Be(new ItemRange(0, 51));
        rangeContext.FilterState.Should().Be(FilterState.Empty);
        rangeContext.SortingState.Should().Be(SortingState.Empty);
    }

    [Fact]
    public async Task Shift_click_upward_outside_the_loaded_window_follows_click_direction()
    {
        // Arrange: a cap makes the surviving membership depend on the direction the range is walked
        var items = BuildItems(60);
        var provider = new RangedItemsProvider(items);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.TestKey)
            .Add(p => p.ItemSelectionAllowed, req => req.CurrentSelection.Count < 3)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);
        await rendered.FindAll("tbody tr")[50].ClickAsync(new MouseEventArgs());

        await MoveLoadedWindowAsync(rendered, startIndex: 0, count: 10);
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().NotBeNull();
        received.Select(x => x.TestKey).Should().ContainInOrder(50, 49, 48);
    }

    [Fact]
    public async Task Shift_click_outside_the_loaded_window_applies_the_selection_veto_to_unrendered_rows()
    {
        // Arrange: row 25 is never rendered by either window, so only the range fetch can reach it
        var items = BuildItems(60);
        var provider = new RangedItemsProvider(items);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.TestKey)
            .Add(p => p.ItemSelectionAllowed, req => req.Item.TestKey != 25)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);
        await rendered.FindAll("tbody tr")[50].ClickAsync(new MouseEventArgs());

        await MoveLoadedWindowAsync(rendered, startIndex: 0, count: 10);
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().HaveCount(50);
        received.Select(x => x.TestKey).Should().NotContain(25);
    }

    [Fact]
    public async Task Shift_click_still_spans_the_range_when_the_provider_ignores_it()
    {
        // Arrange: TestItemsProvider answers every request with the whole set, range or not
        var items = BuildItems(60);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemIdSelector, i => i.TestKey)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 50, count: 10);
        await rendered.FindAll("tbody tr")[50].ClickAsync(new MouseEventArgs());
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().NotBeNull();
        received.Should().HaveCount(51);
        received.Select(x => x.TestKey).Should().BeEquivalentTo(Enumerable.Range(0, 51).Reverse());
    }

    [Fact]
    public async Task Shift_click_after_a_filter_change_selects_only_the_clicked_row()
    {
        // Arrange
        var items = BuildItems(5);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .Add(p => p.Columns, FilterColumn("col")));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());

        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        await rendered.FindAll("tbody tr")[3].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().NotBeNull();
        received.Should().ContainSingle();
    }

    [Fact]
    public async Task Shift_click_after_a_sorting_change_selects_only_the_clicked_row()
    {
        // Arrange
        var items = BuildItems(5);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RangedItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .AddSortableColumn("col"));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());

        await rendered.InvokeAsync(() => rendered.Instance.SetSortingStateAsync(
            SortingState.Empty.WithColumnSorting("col", ascending: false)));

        await rendered.FindAll("tbody tr")[3].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert
        received.Should().NotBeNull();
        received.Should().ContainSingle();
    }

    private static List<TableTestItem> BuildItems(int count)
        => [.. Enumerable.Range(0, count).Select(index => new TableTestItem(index, $"Value {index}"))];

    // bUnit never scrolls, so driving Virtualize's own items-provider parameter is what makes a window that
    // does not start at row zero reachable.
    private static async Task MoveLoadedWindowAsync(IRenderedComponent<AdvancedTable<TableTestItem>> rendered,
        int startIndex, int count)
    {
        var virtualizeComponent = rendered.FindComponent<Virtualization.Virtualize<TableTestItem>>();

        var itemsProvider = virtualizeComponent.Instance.ItemsProvider;
        itemsProvider.Should().NotBeNull();

        await rendered.InvokeAsync(async () =>
            await itemsProvider(new Virtualization.ItemsProviderRequest(startIndex, count, CancellationToken.None)));

        rendered.Render();
    }

    [Fact]
    public async Task Selected_row_has_selected_css_class()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Single));

        rendered.Find("tbody tr").ClassList.Should().NotContain("selected");

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        rendered.Find("tbody tr").ClassList.Should().Contain("selected");
    }

    [Fact]
    public void Selectable_table_has_selectable_css_class()
    {
        // Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.SelectionMode, SelectionMode.Single));

        // Assert
        rendered.Find("table").ClassList.Should().Contain("selectable");
    }

    [Fact]
    public async Task Deleting_item_removes_it_from_selection()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var items = new List<TableTestItem> { item1, item2 };
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Act
        items.Remove(item1);
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        rendered.WaitForAssertion(() => received.Should().BeEmpty());
    }

    [Fact]
    public async Task Adding_item_does_not_change_selection()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var items = new List<TableTestItem> { item1 };
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());
        var selectionAfterClick = received?.ToList();

        // Act
        items.Add(item2);
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        received.Should().BeEquivalentTo(selectionAfterClick);
    }

    [Fact]
    public async Task SelectionMode_multiple_ctrl_click_does_not_mutate_the_bound_list_and_emits_a_new_instance()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var bound = new List<TableTestItem> { item1 };
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItems, bound)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { CtrlKey = true });

        // Assert
        bound.Should().BeEquivalentTo([item1]); // the parameter-bound instance is never mutated in place
        received.Should().BeEquivalentTo([item1, item2]);
        received.Should().NotBeSameAs(bound); // a new list reference is emitted
    }

    [Fact]
    public async Task SelectionMode_multiple_shift_click_matches_the_anchor_by_value_after_a_refetch()
    {
        // Arrange
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value 1"), new(2, "Value 2"), new(3, "Value 3")]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // anchor on the first row, capturing the first-render instance
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());

        // a refetch yields value-equal but reference-distinct instances (records with the same data)
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value 1"), new(2, "Value 2"), new(3, "Value 3")])));

        // Act
        await rendered.FindAll("tbody tr")[2].ClickAsync(new MouseEventArgs { ShiftKey = true });

        // Assert: the value-equal anchor is matched (not duplicated), yielding exactly the three-row range
        received.Should().HaveCount(3);
        received!.Select(i => i.TestKey).Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public async Task Selection_survives_an_unbound_parent_re_render()
    {
        // Arrange: the parent supplies SelectedItems but does not two-way bind it
        var supplied = new List<TableTestItem>();
        var item = new TableTestItem(1, "Value");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItems, supplied));

        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());
        rendered.Find("tbody tr").ClassList.Should().Contain("selected");

        // Act: the parent re-renders, pushing the same (still empty) parameter instance again
        rendered.Render(b => b
            .Add(p => p.SelectedItems, supplied));

        // Assert: the authoritative copy is not reset by the unchanged parameter
        rendered.Find("tbody tr").ClassList.Should().Contain("selected");
    }

    [Fact]
    public void A_refetch_of_equal_items_keeps_a_preserved_cell_subscribed_to_its_row_state()
    {
        // Arrange — the row is keyed by GetRowKey, so a refetch returning equal-but-distinct items preserves
        // the cell, and OnInitialized (where it subscribes to RowState) does not run again. If the table keys
        // its row states by instance, it starts writing to a fresh state the cell never subscribed to: the
        // cascade updates the cell's property, but its handler is still on the old one and never fires.
        var provider = new ReferenceItemsProvider([new(1, "A"), new(2, "B")]);

        var rendered = _testContext.Render<AdvancedTable<ReferenceItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.ItemIdSelector, i => i.Id)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<TableNavigationColumn<ReferenceItem>>(0);
                builder.CloseComponent();
            }));

        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new ReferenceItemsProvider([new(1, "A"), new(2, "B")])));

        var cell = rendered.FindComponents<TableNavigationColumnBodyCellContent<ReferenceItem>>()[0];
        var renderCount = cell.RenderCount;

        // Act — the table raises the hover on whichever row state it holds for this row
        rendered.FindAll("tbody tr")[0].PointerEnter();

        // Assert — the cell heard it, so it is subscribed to the state the table actually writes to
        cell.RenderCount.Should().BeGreaterThan(renderCount);
    }

    [Fact]
    public async Task ItemIdSelector_keeps_the_select_column_checkbox_in_step_with_the_row()
    {
        // Arrange — the sibling of ItemIdSelector_keeps_selection_across_a_fresh_instance_refetch: that one
        // pins the row highlight, this one pins the checkbox inside it. Both read the same selection, so a
        // checkbox testing membership with default equality disagrees with its own row after a refetch.
        var provider = new ReferenceItemsProvider([new(1, "A"), new(2, "B")]);

        var rendered = _testContext.Render<AdvancedTable<ReferenceItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.ItemIdSelector, i => i.Id)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<ReferenceItem>>(0);
                builder.CloseComponent();
            }));

        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        rendered.FindComponents<AdvancedTableSelectColumnBodyCellContent<ReferenceItem>>()[0].Instance.Value.Should().BeTrue();

        // Act — refetch yields brand-new instances with the same ids
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new ReferenceItemsProvider([new(1, "A"), new(2, "B")])));

        // Assert — row and checkbox agree; matching by reference would leave the row selected and the box clear
        rendered.FindAll("tbody tr")[0].ClassList.Should().Contain("selected");
        rendered.FindComponents<AdvancedTableSelectColumnBodyCellContent<ReferenceItem>>()[0].Instance.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ItemIdSelector_keeps_selection_across_a_fresh_instance_refetch()
    {
        // Arrange: a reference-typed item whose default equality is identity, so a refetch breaks it without a key
        var provider = new ReferenceItemsProvider([new(1, "A"), new(2, "B")]);

        var rendered = _testContext.Render<AdvancedTable<ReferenceItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.ItemIdSelector, i => i.Id));

        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        rendered.FindAll("tbody tr")[0].ClassList.Should().Contain("selected");

        // Act: refetch yields brand-new instances with the same ids
        rendered.Render(b => b
            .Add(p => p.ItemsProvider, new ReferenceItemsProvider([new(1, "A"), new(2, "B")])));

        // Assert: the selected row is matched by key, so the highlight survives
        rendered.FindAll("tbody tr")[0].ClassList.Should().Contain("selected");
    }

    [Fact]
    public async Task Changing_the_filter_keeps_filtered_out_rows_selected_in_all_mode()
    {
        // Arrange
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        var provider = new FilterableItemsProvider([item1, item2]);
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.Mode, TableLoadingMode.All)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l))
            .AddSortableColumn("col"));

        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { CtrlKey = true });
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { CtrlKey = true });
        received.Should().BeEquivalentTo([item1, item2]);

        // Act: a filter change hides item2
        provider.HideLast = true;
        await rendered.InvokeAsync(() => rendered.Instance.SetFilterStateAsync(
            FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"))));

        // Assert: the filtered-out row stays selected (prune must not run on a filter change)
        rendered.WaitForAssertion(() => received.Should().BeEquivalentTo([item1, item2]));
    }

    [Fact]
    public void Selection_under_virtualize_without_a_row_id_over_an_unstable_provider_logs_an_error()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        // Act
        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Single));

        // Assert
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    [Fact]
    public void Selection_under_virtualize_with_a_row_id_does_not_log_an_error()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTable<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);

        // Act
        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.ItemIdSelector, i => i.TestKey));

        // Assert
        logger.ReceivedCalls()
            .Should().NotContain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    [Fact]
    public async Task Default_selection_mode_is_multiple_so_clicking_a_row_selects_it()
    {
        // Arrange: no SelectionMode set — the default is now Multiple, so the row-click channel is live
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item]);
    }

    [Fact]
    public async Task Default_selection_mode_is_multiple_so_ctrl_click_accumulates()
    {
        // Arrange: no SelectionMode set — Multiple cardinality applies its modifiers
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs { CtrlKey = true });
        await rendered.FindAll("tbody tr")[1].ClickAsync(new MouseEventArgs { CtrlKey = true });

        // Assert
        received.Should().BeEquivalentTo([item1, item2]);
    }

    [Fact]
    public void RowClickSelectionEnabled_false_still_renders_a_supplied_selection_as_highlighted()
    {
        // Arrange: the row-click channel is display-only — an externally supplied selection must still show as selected
        var item = new TableTestItem(1, "Value");

        // Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItems, [item]));

        // Assert
        rendered.Find("tbody tr").ClassList.Should().Contain("selected");
    }

    [Fact]
    public void Host_can_still_mutate_the_selection_via_binding_while_the_row_click_channel_is_display_only()
    {
        // Arrange: display-only locks out the user, not the host — a re-supplied SelectedItems must update the render
        var item1 = new TableTestItem(1, "Value 1");
        var item2 = new TableTestItem(2, "Value 2");

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item1, item2]))
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItems, [item1]));

        // Act
        rendered.Render(b => b
            .Add(p => p.SelectedItems, [item2]));

        // Assert
        rendered.FindAll("tbody tr")[0].ClassList.Should().NotContain("selected");
        rendered.FindAll("tbody tr")[1].ClassList.Should().Contain("selected");
    }

    [Fact]
    public async Task RowClickSelectionEnabled_false_row_click_does_not_select()
    {
        // Arrange: column-only selection — the row-click channel is switched off while the mode stays Multiple
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await rendered.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeNull();
    }

    [Fact]
    public void RowClickSelectionEnabled_false_omits_the_selectable_css_class()
    {
        // Act: the affordance must track actual row-click behavior, not the mode alone
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false));

        // Assert
        rendered.Find("table").ClassList.Should().NotContain("selectable");
    }

    private sealed class ReferenceItem(int id, string value)
    {
        public int Id => id;
        public string Value => value;
    }

    private sealed class ReferenceItemsProvider(List<ReferenceItem> items) : IItemsProvider<ReferenceItem>
    {
        public Task<ItemsProviderResult<ReferenceItem>> GetItemsAsync(ItemsProviderContext context,
            CancellationToken cancellationToken)
                => Task.FromResult(new ItemsProviderResult<ReferenceItem>(items, items.Count));
    }

    private sealed class FilterableItemsProvider(List<TableTestItem> all)
        : IItemsProvider<TableTestItem>
    {
        public bool HideLast { get; set; }

        public Task<ItemsProviderResult<TableTestItem>> GetItemsAsync(ItemsProviderContext context,
            CancellationToken cancellationToken)
        {
            var items = HideLast ? all.Take(all.Count - 1) : all;

            return Task.FromResult(new ItemsProviderResult<TableTestItem>([.. items], all.Count));
        }
    }
}
