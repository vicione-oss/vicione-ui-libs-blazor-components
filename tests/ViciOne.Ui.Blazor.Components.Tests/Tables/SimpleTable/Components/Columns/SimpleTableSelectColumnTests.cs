using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.AdvancedTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components.Columns;

public sealed class SimpleTableSelectColumnTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public SimpleTableSelectColumnTests()
    {
        _testContext.Services.AddSimpleTable().AddLogging();
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void HasSelectAllHeader_true_by_default_renders_the_select_all_header()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "Content One")];

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.FindComponents<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public void HasSelectAllHeader_false_hides_the_header_while_row_selection_still_works()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "Content One")];

        List<TableTestItem>? selectedItems = null;
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableSelectColumn<>.HasSelectAllHeader), false);
                builder.CloseComponent();
            })
        );

        // Assert: no header rendered
        renderedComponent.FindComponents<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>().Should().BeEmpty();
        renderedComponent.Find("th").TextContent.Should().BeEmpty();

        // Act: per-row selection still works
        var checkbox = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        checkbox.Input(true);

        // Assert
        selectedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Clicking_header_selects_all_or_none()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        List<TableTestItem>? selectedItems = null;
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // ensure first item already is checked
        // Note: selecting cell first to prevent using header
        var cellCheckbox = renderedComponent.FindComponent<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        cellCheckbox.Input(true);

        // Act & assert
        renderedComponent.WaitForState(() => renderedComponent
            .FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Markup.Length > 0);

        var headerCheckbox = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        headerCheckbox.Input(true);

        selectedItems.Should().BeEquivalentTo(items);

        headerCheckbox.Input(false);

        selectedItems.Should().BeEmpty();
    }

    [Fact]
    public void Header_is_updated_on_selection_of_single_items()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "Content One"), new(2, "Content Two")];

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act & Assert
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();

        var firstItemCheckbox = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[0]
            .Find("input[type='checkbox']");

        var secondItemCheckbox = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[1]
            .Find("input[type='checkbox']");

        header.Instance.Value.Should().BeFalse();

        firstItemCheckbox.Input(true);

        header.Instance.Value.Should().BeNull();

        secondItemCheckbox.Input(true);

        header.Instance.Value.Should().BeTrue();

        firstItemCheckbox.Input(false);

        header.Instance.Value.Should().BeNull();

        secondItemCheckbox.Input(false);

        header.Instance.Value.Should().BeFalse();
    }

    [Fact]
    public void Header_is_updated_on_change_of_items()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "Content One"), new(2, "Content Two")];

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act & Assert
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();

        var headerCheckbox = header.Find("input[type='checkbox']");
        headerCheckbox.Input(true);

        header.Instance.Value.Should().BeTrue();

        // delete a selected -> results in all items selected
        renderedComponent.Render(b => b
            .Add(p => p.Items, [.. items.Take(1)])
        );

        header.Instance.Value.Should().BeTrue();

        // readd the removed -> readded is NOT selected -> partial selection
        renderedComponent.Render(b => b
            .Add(p => p.Items, [.. items])
        );

        header.Instance.Value.Should().BeNull();

        // remove the only non-selected -> all are selected
        renderedComponent.Render(b => b
            .Add(p => p.Items, [.. items.Take(1)])
        );

        header.Instance.Value.Should().BeTrue();

        // remove the only remaining -> none are selected
        renderedComponent.Render(b => b
            .Add(p => p.Items, [])
        );

        header.Instance.Value.Should().BeFalse();
    }

    [Fact]
    public void Header_selects_all_updates_table_level_selected_items()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
        };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Act
        var headerCheckbox = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']");
        headerCheckbox.Input(true);

        // Assert
        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Header_checkbox_is_disabled_when_a_veto_is_configured()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "One"), new(2, "Two")];

        // Act: select-all has no coherent meaning under a constraint, so the header is disabled
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.ItemSelectionAllowed, _ => true)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Instance.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Header_checkbox_is_disabled_in_single_mode()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "One"), new(2, "Two")];

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();
        header.Instance.Enabled.Should().BeFalse();
        header.Instance.Value.Should().BeFalse();
    }

    [Fact]
    public void Single_mode_header_reflects_a_partial_selection_as_indeterminate_while_disabled()
    {
        // Arrange: the select-all header is disabled under Single, but must still show that something is selected
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        List<TableTestItem> items = [item1, item2];

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItems, [item1])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();
        header.Instance.Enabled.Should().BeFalse();
        header.Instance.Value.Should().BeNull();
    }

    [Fact]
    public void DisplayOnly_column_disables_the_select_all_header()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "One"), new(2, "Two")];

        // Act: a display-only column — the header renders but cannot mutate the selection
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Instance.Enabled.Should().BeFalse();
    }

    [Fact]
    public void DisplayOnly_column_header_still_reflects_a_supplied_full_selection()
    {
        // Arrange: display-only preserved — the disabled header still shows the correct tri-state
        var item1 = new TableTestItem(1, "One");
        var item2 = new TableTestItem(2, "Two");
        List<TableTestItem> items = [item1, item2];

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectedItems, [item1, item2])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Assert
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();
        header.Instance.Enabled.Should().BeFalse();
        header.Instance.Value.Should().BeTrue();
    }

    [Fact]
    public void DisplayOnly_column_forcing_the_header_does_not_change_the_selection()
    {
        // Arrange
        List<TableTestItem> items = [new(1, "One"), new(2, "Two")];

        List<TableTestItem>? selectedItems = null;
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => selectedItems = l))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableSelectColumn<>.DisplayOnly), true);
                builder.CloseComponent();
            })
        );

        // Act: force the disabled header checkbox — the column's DisplayOnly guard must still block it
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").Input(true);

        // Assert
        selectedItems.Should().BeNull();
    }

    [Fact]
    public async Task Filtering_never_deselects_individually_selected_rows()
    {
        // Arrange — individual selection is data-scoped and survives a filter round-trip
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var items = new List<TableTestItem> { apple, banana };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, SelectAndValueColumns()));

        var cells = renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>();
        await cells[0].Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = true });
        await cells[1].Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = true });

        // Act — filter hides Apple, then the filter is removed again
        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(ValueContains("ban")));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().ContainSingle());

        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(FilterState.Empty));

        // Assert — both rows are still selected once revealed
        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(2));

        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo([apple, banana]);
    }

    [Fact]
    public async Task Select_all_under_a_filter_covers_the_filtered_set_and_leaves_hidden_rows_unselected()
    {
        // Arrange — bulk selection acts on the Filtered Set only
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var apricot = new TableTestItem(3, "Apricot");
        var items = new List<TableTestItem> { apple, banana, apricot };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.FilterState, ValueContains("ap"))
            .Add(p => p.Columns, SelectAndValueColumns()));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(2));

        // Act — select-all over the Filtered Set
        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();
        await header.Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = true });

        // Assert — the header lands on checked (the tri-state is scoped to the Filtered Set, so no dash for
        // hidden rows) and removing the filter reveals the hidden row unticked
        header.Instance.Value.Should().BeTrue();

        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(FilterState.Empty));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(3));

        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo([apple, apricot]);
    }

    [Fact]
    public async Task Deselect_all_under_a_filter_clears_only_the_filtered_set_and_keeps_hidden_selections()
    {
        // Arrange — deselect-all clears the visible set; hidden selections are untouched
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var apricot = new TableTestItem(3, "Apricot");
        var items = new List<TableTestItem> { apple, banana, apricot };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, SelectAndValueColumns()));

        await renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = true });

        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo(items);

        // Act — filter to the "ap" rows, deselect-all, then remove the filter
        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(ValueContains("ap")));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(2));

        await renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = false });

        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(FilterState.Empty));

        // Assert — only the row the filter had hidden is still selected
        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(3));

        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo([banana]);
    }

    [Fact]
    public async Task Header_is_unchecked_when_the_only_selections_are_hidden_by_the_filter()
    {
        // Arrange — the tri-state reflects the Filtered Set only: a hidden selection must not force the dash
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var items = new List<TableTestItem> { apple, banana };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, SelectAndValueColumns()));

        await renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[1]
            .Find("input[type='checkbox']").InputAsync(new ChangeEventArgs { Value = true });

        // Act — the filter hides the selected Banana
        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(ValueContains("ap")));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().ContainSingle());

        // Assert — nothing visible is selected, so the header is unchecked, not indeterminate
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Instance.Value.Should().BeFalse();
    }

    [Fact]
    public void Header_is_indeterminate_when_the_filtered_set_is_partially_selected()
    {
        // Arrange
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var apricot = new TableTestItem(3, "Apricot");
        var items = new List<TableTestItem> { apple, banana, apricot };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.FilterState, ValueContains("ap"))
            .Add(p => p.Columns, SelectAndValueColumns()));

        renderedComponent.WaitForAssertion(() => renderedComponent
            .FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>().Should().HaveCount(2));

        // Act — select one of the two visible rows
        renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[0]
            .Find("input[type='checkbox']").Input(true);

        // Assert
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Instance.Value.Should().BeNull();
    }

    [Fact]
    public void Select_all_does_not_duplicate_rows_that_are_already_selected()
    {
        // Arrange — one row selected by hand before the bulk action
        var apple = new TableTestItem(1, "Apple");
        var banana = new TableTestItem(2, "Banana");
        var apricot = new TableTestItem(3, "Apricot");
        var items = new List<TableTestItem> { apple, banana, apricot };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, SelectAndValueColumns()));

        renderedComponent.FindComponents<AdvancedTableSelectColumnBodyCellContent<TableTestItem>>()[1]
            .Find("input[type='checkbox']").Input(true);

        // Act — select-all covers that row again
        renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>()
            .Find("input[type='checkbox']").Input(true);

        // Assert — every row is selected exactly once
        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo([apple, banana, apricot]);
        renderedComponent.Instance.SelectedItems.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Select_all_under_virtualize_selects_the_whole_filtered_set()
    {
        // Arrange — under Virtualize every provide is ranged (a render window), yet select-all must cover
        // the whole Filtered Set, not just the rendered rows: the provider caches it on each provide,
        // independent of the requested range.
        var items = Enumerable.Range(1, 10)
            .Select(i => new TableTestItem(i, i <= 6 ? $"Match {i}" : $"Other {i}"))
            .ToList();

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Mode, TableLoadingMode.Virtualize)
            .Add(p => p.FilterState, ValueContains("match"))
            .Add(p => p.Columns, SelectAndValueColumns()));

        var header = renderedComponent.FindComponent<SimpleTableSelectColumnHeaderCellContent<TableTestItem>>();
        renderedComponent.WaitForAssertion(() => header.Instance.Enabled.Should().BeTrue());

        // Act
        header.Find("input[type='checkbox']").Input(true);

        // Assert — every filtered row is selected, including the ones Virtualize never rendered
        renderedComponent.Instance.SelectedItems.Should().BeEquivalentTo(items.Take(6));
    }

    [Fact]
    public void Guard_throws_when_placed_outside_a_simple_table()
    {
        // Arrange
        using var localContext = new BunitContext();
        localContext.Services.AddAdvancedTable();
        localContext.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var act = () => localContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
                builder.CloseComponent();
            })
        );

        // Assert
        var exception = act.Should().Throw<InvalidOperationException>().Which;
        exception.Message.Should().Contain(nameof(ISimpleTable<>));
    }

    [Fact]
    public void A_disposed_select_column_stops_refreshing_on_a_provide()
    {
        // Arrange — the column is rendered on its own against substituted tables, so the test decides when
        // the table reports items and can raise that after the column is gone.
        var table = Substitute.For<IAdvancedTable<TableTestItem>>();
        var simpleTable = Substitute.For<ISimpleTable<TableTestItem>>();

        var renderedComponent = _testContext.Render<SimpleTableSelectColumn<TableTestItem>>(b => b
            .AddCascadingValue(table)
            .AddCascadingValue(simpleTable));

        var columnState = new ColumnState();
        ((IAdvancedTableColumn<TableTestItem>)renderedComponent.Instance).SetState(columnState);

        var refreshed = false;
        columnState.RefreshRequested += () => refreshed = true;

        renderedComponent.Instance.Dispose();

        // Act — the table reports a new set of items after the column is gone
        table.ItemsChanged += Raise.Event<Action>();

        // Assert
        refreshed.Should().BeFalse();
    }

    // A select column plus a filterable value column (Id "Value") — the filter tests target that id.
    private static RenderFragment SelectAndValueColumns()
        => builder =>
        {
            builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
            builder.CloseComponent();

            builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(1);
            builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.Id), "Value");
            builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.CellContent),
                (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
            builder.CloseComponent();
        };

    private static FilterState ValueContains(string term)
        => FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("Value", term, x => x.TestValue));
}
