using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Columns;

public sealed class AdvancedTableSelectColumnTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public AdvancedTableSelectColumnTests()
    {
        _testContext.Services.AddAdvancedTable();
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Renders_a_cell_per_item()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(items.Count);
    }

    [Fact]
    public void Renders_at_the_minimum_width()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One") };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.Find("col").GetAttribute("style").Should().Be("width: 52px");
    }

    [Fact]
    public void No_header_is_rendered()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One") };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.Find("th").TextContent.Should().BeEmpty();
        renderedComponent.FindAll("thead input[type='checkbox']").Should().BeEmpty();
    }

    [Fact]
    public void Selecting_a_cell_updates_selectedItems()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One") };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act & Assert
        var checkbox = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        checkbox.Input(true);

        selectedItems.Should().BeEquivalentTo(items);

        checkbox.Input(false);

        selectedItems.Should().BeEmpty();
    }

    [Fact]
    public void Selecting_a_cell_does_not_style_the_row_differently_from_a_row_click()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        var rows = renderedComponent.FindAll("tbody tr");
        rows.Should().HaveCount(2);

        // Act
        renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .ElementAt(0).Find("input[type='checkbox']").Input(true);

        renderedComponent.FindAll("tbody tr").ElementAt(1).Click();

        // Assert
        var cellsAfter = renderedComponent.FindAll("tbody td");
        cellsAfter.Should().NotBeEmpty();
        cellsAfter.Should().AllSatisfy(cell => cell.ClassList.Should().NotContain("active"));
    }

    [Fact]
    public void Selecting_a_cell_requests_a_column_refresh()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One") };

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act & Assert
        var cellContainer = renderedComponent.FindComponents<CellContainer>().ElementAt(1); // 0 is header

        var refreshRequests = 0;
        cellContainer.Instance.ColumnState.RefreshRequested += () => refreshRequests++;

        var checkbox = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        checkbox.Input(true);

        refreshRequests.Should().Be(1);

        checkbox.Input(false);

        refreshRequests.Should().Be(2);
    }

    [Fact]
    public void Cell_is_disabled_when_veto_rejects_the_item()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, request => request.Item.TestKey != 2) // item 2 cannot be selected
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        var cells = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>();
        cells[0].Instance.Enabled.Should().BeTrue();
        cells[1].Instance.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Toggling_a_vetoed_item_does_not_select_it()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One"), new(2, "Two") };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.ItemSelectionAllowed, request => request.Item.TestKey != 2) // item 2 cannot be selected
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act: force the (disabled) checkbox — the veto must still reject on commit
        var vetoedCheckbox = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[1]
            .Find("input[type='checkbox']");
        vetoedCheckbox.Input(true);

        // Assert
        selectedItems.Should().BeNull();
    }

    [Fact]
    public void Single_mode_selecting_a_cell_replaces_the_previously_selected()
    {
        // Arrange
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var items = new List<TableTestItem> { item1, item2 };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        var cells = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>();

        // Act
        cells[0].Find("input[type='checkbox']").Input(true);
        cells[1].Find("input[type='checkbox']").Input(true);

        // Assert
        selectedItems.Should().BeEquivalentTo([item2]);
    }

    [Fact]
    public void Single_mode_toggling_selected_cell_off_clears_selection()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "One") };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        var checkbox = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");

        // Act
        checkbox.Input(true);
        checkbox.Input(false);

        // Assert
        selectedItems.Should().BeEmpty();
    }

    [Fact]
    public void DisplayOnly_column_renders_the_cell_disabled_but_still_checked_for_a_supplied_selection()
    {
        // Arrange: a display-only column reflects the supplied selection but its checkbox cannot be toggled
        var item = new TableTestItem(1, "One");
        var items = new List<TableTestItem> { item };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectedItems, [item])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Assert
        var cell = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>();
        cell.Instance.Enabled.Should().BeFalse();
        cell.Instance.Value.Should().BeTrue();
    }

    [Fact]
    public void DisplayOnly_column_forcing_the_cell_does_not_change_the_selection()
    {
        // Arrange: even if the disabled checkbox is forced, a display-only column cannot mutate the selection
        var items = new List<TableTestItem> { new(1, "One") };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Act
        renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").Input(true);

        // Assert
        selectedItems.Should().BeNull();
    }

    [Fact]
    public async Task DisplayOnly_column_with_row_click_on_still_selects_via_the_row_while_the_cell_stays_disabled()
    {
        // Arrange: the two channels are independent — a display-only column does not disable the row-click channel
        var item = new TableTestItem(1, "One");
        var items = new List<TableTestItem> { item };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Act
        await renderedComponent.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        selectedItems.Should().BeEquivalentTo([item]);
        renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Instance.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Column_only_selection_the_cell_still_selects_when_row_click_is_disabled()
    {
        // Arrange: RowClickSelectionEnabled=false must not touch the column channel — the cell keeps selecting
        var items = new List<TableTestItem> { new(1, "One") };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act
        renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").Input(true);

        // Assert
        selectedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Column_only_single_cardinality_a_second_cell_replaces_the_selection()
    {
        // Arrange: the Single + column-only combination is reachable only via RowClickSelectionEnabled=false
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        var items = new List<TableTestItem> { item1, item2 };
        List<TableTestItem>? selectedItems = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        var cells = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>();

        // Act
        cells[0].Find("input[type='checkbox']").Input(true);
        cells[1].Find("input[type='checkbox']").Input(true);

        // Assert
        selectedItems.Should().BeEquivalentTo([item2]);
    }
}
