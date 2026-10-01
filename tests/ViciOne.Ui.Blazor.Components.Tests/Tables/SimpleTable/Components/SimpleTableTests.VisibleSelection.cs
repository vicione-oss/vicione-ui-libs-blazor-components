using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public void Initial_value_is_reported_before_any_interaction_and_covers_the_whole_selection()
    {
        // Arrange
        var items = BuildVisibleSelectionItems();
        List<TableTestItem> selectedItems = [items[0], items[1]];

        var raised = new List<string>();
        IReadOnlyList<TableTestItem>? visibleSelection = null;
        IReadOnlyList<TableTestItem>? filteredItems = null;

        // Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.SelectedItems, selectedItems)
            .AddColumns()
            .AddVisibleSelectionRecording(raised, list => filteredItems = list, list => visibleSelection = list));

        // Assert
        rendered.WaitForAssertion(() => visibleSelection.Should().NotBeNull());

        visibleSelection.Should().BeEquivalentTo([items[0], items[1]]);
        filteredItems.Should().BeEquivalentTo(items);
        raised.Should().EndWith(["visible"]);
    }

    [Fact]
    public void Filter_change_reports_the_filtered_items_and_the_visible_selection_last()
    {
        // Arrange
        var items = BuildVisibleSelectionItems();
        List<TableTestItem> selectedItems = [items[0], items[3]];

        var raised = new List<string>();
        IReadOnlyList<TableTestItem>? visibleSelection = null;
        IReadOnlyList<TableTestItem>? filteredItems = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.SelectedItems, selectedItems)
            .AddColumns()
            .AddVisibleSelectionRecording(raised, list => filteredItems = list, list => visibleSelection = list));

        rendered.WaitForAssertion(() => visibleSelection.Should().NotBeNull());
        raised.Clear();

        // Act
        rendered.Render(b => b
            .Add(p => p.FilterState, ValueContainsFilter("Match")));

        // Assert
        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());

        filteredItems.Should().BeEquivalentTo([items[0], items[1]]);
        visibleSelection.Should().BeEquivalentTo([items[0]]);
        rendered.Instance.SelectedItems.Should().BeEquivalentTo([items[0], items[3]]);
        raised.Should().Equal(["filtered", "visible"]);
    }

    [Fact]
    public async Task Selection_change_inside_the_table_reports_the_visible_selection_without_reporting_filtered_items()
    {
        // Arrange
        var items = BuildVisibleSelectionItems();

        var raised = new List<string>();
        IReadOnlyList<TableTestItem>? visibleSelection = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .AddColumns()
            .AddVisibleSelectionRecording(raised, _ => { }, list => visibleSelection = list));

        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());
        raised.Clear();

        // Act
        await rendered.FindAll("tbody tr")[2].ClickAsync(new MouseEventArgs());

        // Assert
        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());

        visibleSelection.Should().BeEquivalentTo([items[2]]);
        raised.Should().Equal(["visible"]);
    }

    [Fact]
    public void Selection_assigned_from_outside_reports_the_visible_selection()
    {
        // Arrange: the wrapped table adopts a replaced selection parameter without raising anything of its
        // own, so the reference change is the only evidence a bulk-select action leaves
        var items = BuildVisibleSelectionItems();

        var raised = new List<string>();
        IReadOnlyList<TableTestItem>? visibleSelection = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns()
            .AddVisibleSelectionRecording(raised, _ => { }, list => visibleSelection = list));

        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());
        raised.Clear();

        // Act
        rendered.Render(b => b
            .Add(p => p.SelectedItems, [items[1], items[2]]));

        // Assert
        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());

        visibleSelection.Should().BeEquivalentTo([items[1], items[2]]);
        raised.Should().Equal(["visible"]);
    }

    [Fact]
    public async Task Moving_the_loaded_window_reports_nothing_and_leaves_the_visible_selection_unchanged()
    {
        // Arrange: under virtualization every window refill is a provide
        var items = Enumerable.Range(1, 60)
            .Select(i => new TableTestItem(i, $"Value {i}"))
            .ToList();

        var raised = new List<string>();
        IReadOnlyList<TableTestItem>? visibleSelection = null;

        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .AddColumns()
            .AddVisibleSelectionRecording(raised, _ => { }, list => visibleSelection = list));

        rendered.WaitForAssertion(() => raised.Should().NotBeEmpty());

        await rendered.FindAll("tbody tr")[0].ClickAsync(new MouseEventArgs());
        rendered.WaitForAssertion(() => visibleSelection.Should().NotBeEmpty());

        raised.Clear();

        // Act
        await MoveLoadedWindowAsync(rendered, startIndex: 40, count: 10);

        // Assert
        raised.Should().BeEmpty();
        visibleSelection.Should().BeEquivalentTo([items[0]]);
    }

    [Fact]
    public void Simple_table_exposes_no_item_id_selector_so_the_visible_selection_may_compare_by_default_equality()
    {
        // Arrange: the intersection uses a plain set, which matches the wrapped table's comparer only while
        // nothing supplies an item-id selector

        // Act
        var itemIdSelector = typeof(SimpleTable<TableTestItem>).GetProperty("ItemIdSelector");

        // Assert
        itemIdSelector.Should().BeNull();
    }

    private static List<TableTestItem> BuildVisibleSelectionItems()
        => [new(1, "Match One"), new(2, "Match Two"), new(3, "Other Three"), new(4, "Other Four")];

    private static FilterState ValueContainsFilter(string value)
        => FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", value, x => x.TestValue));

    // bUnit never scrolls, so driving Virtualize's own items-provider parameter is what makes a moved window
    // reachable.
    private static async Task MoveLoadedWindowAsync(IRenderedComponent<SimpleTable<TableTestItem>> rendered,
        int startIndex, int count)
    {
        var virtualizeComponent = rendered.FindComponent<Virtualize<TableTestItem>>();

        var itemsProvider = virtualizeComponent.Instance.ItemsProvider;
        itemsProvider.Should().NotBeNull();

        await rendered.InvokeAsync(async () =>
            await itemsProvider(new ItemsProviderRequest(startIndex, count, CancellationToken.None)));

        rendered.Render();
    }
}
