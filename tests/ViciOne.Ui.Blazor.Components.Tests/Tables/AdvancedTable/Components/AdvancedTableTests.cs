using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Headers;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Panes;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

#pragma warning disable RCS1060 // Declare each type in separate file
public sealed partial class AdvancedTableTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();
    private readonly TestDragInteraction _fakeDragInteraction = new();

    public AdvancedTableTests()
    {
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        _testContext.Services.AddLogging();
        _testContext.Services.AddScoped<IDragInteraction>(_ => _fakeDragInteraction);
        _testContext.Services.AddAdvancedTable();
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Should_render_without_items_and_without_any_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>();

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Fact]
    public void Should_render_with_items_and_without_any_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableHeader>().Should().BeEmpty();
        renderedComponent.FindComponents<LeftPane>().Should().BeEmpty();
        renderedComponent.FindComponents<AdvancedTableColumn<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<CellContainer>().Should().HaveCount(0);
    }

    [Fact]
    public void Should_render_the_css_class_on_the_root_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.CssClass, "custom-one custom-two"));

        // Assert
        renderedComponent.Find(".advanced-table").ClassList
            .Should().BeEquivalentTo("advanced-table", "custom-one", "custom-two");
    }

    [Fact]
    public void Should_render_only_the_own_class_on_the_root_element_without_css_class()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert
        renderedComponent.Find(".advanced-table").ClassList.Should().BeEquivalentTo("advanced-table");
    }

    [Fact]
    public void Should_render_the_maximum_height_as_style_on_the_root_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.MaximumHeight, "300px"));

        // Assert
        renderedComponent.Find(".advanced-table").GetAttribute("style").Should().Be("max-height: 300px");
    }

    [Fact]
    public void Should_render_with_items_and_columns()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .AddColumns()
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableHeader>().Should().BeEmpty();
        renderedComponent.FindComponents<LeftPane>().Should().BeEmpty();
        renderedComponent.FindComponents<AdvancedTableColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<CellContainer>().Should().HaveCount((items.Count + 1) * 2); // one per item per column plus the header
    }

    [Fact]
    public void Should_render_with_items_and_all_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .AddHeader()
            .AddLeftPane()
            .AddColumns()
            .AddFooter()
        );

        // Assert
        var a = renderedComponent.FindComponents<CellContainer>();

        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableHeader>().Should().ContainSingle();
        renderedComponent.FindComponents<LeftPane>().Should().ContainSingle();
        renderedComponent.FindComponents<AdvancedTableColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<CellContainer>().Should().HaveCount((items.Count + 1) * 2); // one per item per column plus the header
    }

    [Fact]
    public void Should_render_with_no_items_and_all_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>();

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .AddHeader()
            .AddLeftPane()
            .AddColumns()
            .AddFooter()
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableHeader>().Should().ContainSingle();
        renderedComponent.FindComponents<LeftPane>().Should().ContainSingle();
        renderedComponent.FindComponents<AdvancedTableColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<CellContainer>().Should().HaveCount((items.Count + 1) * 2); // one per item per column plus the header
    }

    [Fact]
    public void Throws_exception_on_duplicate_column_ids()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Content One") };
        const string Id = "column id";

        // Act
        var result = () => _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(
                    1, nameof(AdvancedTableColumn<>.Id),
                    Id);
                builder.CloseComponent();

                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(2);
                builder.AddComponentParameter(
                    3, nameof(AdvancedTableColumn<>.Id),
                    Id);
                builder.CloseComponent();
            })
        );

        // Assert
        result.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Removes_column_from_table_when_unrendered()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Content One") };

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "first");
                builder.CloseComponent();

                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(2);
                builder.AddComponentParameter(3, nameof(AdvancedTableColumn<>.Id), "second");
                builder.CloseComponent();
            })
        );

        renderedComponent.FindAll("th").Should().HaveCount(2);

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "first");
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.WaitForAssertion(() => renderedComponent.FindAll("th").Should().HaveCount(1));
        renderedComponent.FindComponents<AdvancedTableColumn<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public async Task Disposing_context_with_table_and_columns_does_not_throw()
    {
        // Arrange
        await using var localContext = new BunitContext();
        localContext.Services.AddAdvancedTable();
        localContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var items = new List<TableTestItem> { new(1, "Content One") };

        localContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "first");
                builder.CloseComponent();
            })
        );

        // Act & Assert
        Func<Task> act = () => localContext.DisposeAsync().AsTask();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Clicking_sortable_column_header_invokes_sorting_changed_with_ascending()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        SortingState? lastSorting = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SortingStateChanged, s => lastSorting = s)
            .AddSortableColumn("col"));

        // Act
        renderedComponent.Find(".sortable").Click();

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            lastSorting.Should().NotBeNull();
            lastSorting.ColumnSortings.Should().ContainSingle().Which.Should().Be(new ColumnSorting("col", true));
        });
    }

    [Fact]
    public void Clicking_sortable_column_header_twice_toggles_sort_direction()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        SortingState? lastSorting = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SortingStateChanged, s => lastSorting = s)
            .AddSortableColumn("col"));

        renderedComponent.Find(".sortable").Click();
        renderedComponent.WaitForAssertion(() => lastSorting.Should().NotBeNull());

        // Act
        renderedComponent.Find(".sortable").Click();

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            lastSorting.Should().NotBeNull();
            lastSorting.ColumnSortings.Should().ContainSingle().Which.Should().Be(new ColumnSorting("col", false));
        });
    }

    [Fact]
    public void Clicking_non_sortable_header_does_not_invoke_sorting_changed()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        var sortingChangedCalled = false;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SortingStateChanged, _ => sortingChangedCalled = true)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                builder.CloseComponent();
            }));

        // Act
        renderedComponent.Find(".header-cell").Click();

        // Assert
        sortingChangedCalled.Should().BeFalse();
    }

    [Fact]
    public void Pressing_enter_on_sortable_header_invokes_sorting_changed()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        SortingState? lastSorting = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SortingStateChanged, s => lastSorting = s)
            .AddSortableColumn("col"));

        // Act
        renderedComponent.Find(".sortable").KeyUp(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            lastSorting.Should().NotBeNull();
            lastSorting.ColumnSortings.Should().ContainSingle()
                .Which.Should().Be(new ColumnSorting("col", true));
        });
    }

    [Fact]
    public void Pressing_non_enter_key_on_sortable_header_does_not_invoke_sorting_changed()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        var sortingChangedCalled = false;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.SortingStateChanged, _ => sortingChangedCalled = true)
            .AddSortableColumn("col"));

        // Act
        renderedComponent.Find(".sortable").KeyUp(new KeyboardEventArgs { Key = "Space" });

        // Assert
        sortingChangedCalled.Should().BeFalse();
    }

    [Fact]
    public void Unsorted_column_has_aria_sort_none()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                builder.CloseComponent();
            }));

        // Assert
        renderedComponent.Find("th").GetAttribute("aria-sort").Should().Be("none");
    }

    [Fact]
    public void Sorted_ascending_column_has_aria_sort_ascending()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("col", true))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                builder.CloseComponent();
            }));

        // Assert
        renderedComponent.Find("th").GetAttribute("aria-sort").Should().Be("ascending");
    }

    [Fact]
    public void Sorted_descending_column_has_aria_sort_descending()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("col", false))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                builder.CloseComponent();
            }));

        // Assert
        renderedComponent.Find("th").GetAttribute("aria-sort").Should().Be("descending");
    }

    [Fact]
    public void Items_provider_receives_current_sorting_in_context()
    {
        // Arrange — the column has to exist, otherwise the sorting names nothing the table has and is dropped
        var sorting = SortingState.Empty.WithColumnSorting("col", true);
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        // Act
        _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.SortingState, sorting)
            .AddSortableColumn("col"));

        // Assert
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.SortingState.Should().Be(sorting);
    }

    [Fact]
    public void Items_provider_receives_current_filter_in_context()
    {
        // Arrange — the column has to exist, otherwise the filter names nothing the table has and is dropped
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("col"));
        var provider = new RecordingItemsProvider([new(1, "Value")]);

        // Act
        _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .Add(p => p.FilterState, filter)
            .AddSortableColumn("col"));

        // Assert
        provider.LastContext.Should().NotBeNull();
        provider.LastContext.FilterState.Should().Be(filter);
    }

    [Fact]
    public void Items_are_refetched_when_items_provider_changes()
    {
        // Arrange
        var provider1 = new TestItemsProvider([new(1, "Value 1")]);
        var provider2 = new TestItemsProvider([new(2, "Value 2"), new(3, "Value 3")]);

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider1));

        renderedComponent.FindAll("tbody tr").Should().HaveCount(1);

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ItemsProvider, provider2));

        // Assert
        renderedComponent.WaitForAssertion(() =>
            renderedComponent.FindAll("tbody tr").Should().HaveCount(2));
    }

    [Fact]
    public void Items_are_rendered_as_rows()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Value 1"),
            new(2, "Value 2"),
            new(3, "Value 3")
        };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        renderedComponent.FindAll("tbody tr").Should().HaveCount(items.Count);
    }

    [Fact]
    public void Maximum_height_renders_as_style_on_wrapper()
    {
        // Arrange
        const string MaxHeight = "300px";

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.MaximumHeight, MaxHeight));

        // Assert
        renderedComponent.Find(".advanced-table").GetAttribute("style")
            .Should().Be($"max-height: {MaxHeight}");
    }

    [Fact]
    public void Custom_footer_renders_instead_of_the_built_in_footer()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .Add(p => p.Footer, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-footer");
                builder.CloseElement();
            }));

        // Assert
        renderedComponent.FindAll(".custom-footer").Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
    }

    [Fact]
    public void Unsupplied_footer_emits_no_footer_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert: the wrapper carries padding, so leaving it out is what stops it reserving space
        renderedComponent.FindAll(".footer").Should().BeEmpty();
    }

    [Fact]
    public void Supplied_footer_emits_a_footer_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .AddFooter());

        // Assert
        renderedComponent.FindAll(".footer").Should().ContainSingle();
    }

    [Fact]
    public void Unsupplied_header_emits_no_header_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert
        renderedComponent.FindAll(".header").Should().BeEmpty();
    }

    [Fact]
    public void Supplied_header_emits_a_header_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .AddHeader());

        // Assert
        renderedComponent.FindAll(".header").Should().ContainSingle();
    }

    [Fact]
    public void Unsupplied_left_outlet_emits_no_left_outlet_element()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert
        renderedComponent.FindAll(".left-outlet").Should().BeEmpty();
    }

    [Fact]
    public void Get_total_item_count_returns_total_from_provider()
    {
        // Arrange
        const int TotalCount = 100;
        var provider = new RecordingItemsProvider([], TotalCount);

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider));

        // Assert
        var table = (IAdvancedTable<TableTestItem>)renderedComponent.Instance;
        table.GetTotalItemCount().Should().Be(TotalCount);
    }

    [Fact]
    public void Virtualize_mode_renders_virtualize_component()
    {
        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.MaximumHeight, "300px")
            .Add(p => p.Mode, TableLoadingMode.Virtualize));

        // Assert
        renderedComponent.FindComponents<Virtualize<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public async Task Double_clicking_a_row_invokes_row_double_click_with_the_item()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        TableTestItem? receivedItem = null;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.RowDoubleClick, i => receivedItem = i));

        // Act
        await renderedComponent.Find("tbody tr").DoubleClickAsync();

        // Assert
        receivedItem.Should().Be(item);
    }

    [Fact]
    public async Task Double_clicking_a_row_without_row_double_click_does_not_throw()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")])));

        // Act
        var act = async () => await renderedComponent.Find("tbody tr").DoubleClickAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Rows_draggable_false_does_not_attach_row_to_drag_interaction()
    {
        // Arrange & Act
        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, false));

        // Assert: drag is JS-pointer-based, so a non-draggable row is never attached to the interaction
        _fakeDragInteraction.LastAttached.Should().BeNull();
    }

    [Fact]
    public void Rows_draggable_true_attaches_row_to_drag_interaction()
    {
        // Arrange & Act
        _ = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        // Assert: drag is JS-pointer-based, so a draggable row attaches itself to the interaction
        _fakeDragInteraction.LastAttached.Should().NotBeNull();
        _fakeDragInteraction.LastAttached!.Draggable.Should().BeTrue();
    }

    [Fact]
    public async Task Toggling_rows_draggable_off_while_attach_is_in_flight_still_detaches_the_row()
    {
        // Arrange: gate the attach so it stays in flight when draggable is toggled off
        var attachGate = new TaskCompletionSource();
        _fakeDragInteraction.AttachGate = attachGate;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        _fakeDragInteraction.AttachCount.Should().Be(1);
        _fakeDragInteraction.RemoveCount.Should().Be(0);

        // Act: request detach while the attach is still pending, then let the attach complete
        renderedComponent.Render(b => b
            .Add(p => p.RowsDraggable, false));
        attachGate.SetResult();

        // Assert: detach is unconditional, so the row is removed even though the attach was in flight. The
        // removal is awaited rather than waited for through a render, because releasing the gate resumes the
        // detach on the renderer's dispatcher without producing one.
        await _fakeDragInteraction.Removed.WaitAsync(TimeSpan.FromSeconds(10),
            Xunit.TestContext.Current.CancellationToken);

        _fakeDragInteraction.RemoveCount.Should().Be(1);
    }

    [Fact]
    public void Faulted_attach_is_reset_so_a_later_render_can_retry()
    {
        // Arrange: the first attach faults
        _fakeDragInteraction.FaultNextAttach = true;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        _fakeDragInteraction.AttachCount.Should().Be(1);
        _fakeDragInteraction.LastAttached.Should().BeNull();

        // Act: a later render must retry because the faulted attach was reset
        renderedComponent.Render(b => b
            .Add(p => p.RowsDraggable, true));

        // Assert
        renderedComponent.WaitForAssertion(() =>
        {
            _fakeDragInteraction.AttachCount.Should().Be(2);
            _fakeDragInteraction.LastAttached.Should().NotBeNull();
        });
    }

    [Fact]
    public void A_faulted_attach_recovered_by_a_later_render_logs_no_error()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTableRow<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);
        _fakeDragInteraction.FaultNextAttach = true;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.RowsDraggable, true));

        // Assert
        renderedComponent.WaitForAssertion(() => _fakeDragInteraction.LastAttached.Should().NotBeNull());
        logger.ReceivedCalls()
            .Should().NotContain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    [Fact]
    public void A_faulted_attach_with_no_attach_succeeding_in_between_logs_an_error()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AdvancedTableRow<TableTestItem>>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _testContext.Services.AddSingleton(logger);
        _fakeDragInteraction.FaultNextAttach = true;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowsDraggable, true));

        // Act
        _fakeDragInteraction.FaultNextAttach = true;
        renderedComponent.Render(b => b
            .Add(p => p.RowsDraggable, true));

        // Assert
        renderedComponent.WaitForAssertion(() => _fakeDragInteraction.AttachCount.Should().Be(2));
        logger.ReceivedCalls()
            .Should().Contain(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel)call.GetArguments()[0]! == LogLevel.Error);
    }

    // Writes the column's state exactly as the column chooser does, so behavior that follows from hiding a
    // column is verified against the path production takes instead of a hand-rolled equivalent of it. The
    // column is addressed by its rendered header, the same label a user picks in the chooser menu.
    private async Task SetColumnVisibilityAsync(
        IRenderedComponent<AdvancedTable<TableTestItem>> rendered,
        string columnTitle,
        bool visible)
    {
        var column = ((IAdvancedTable<TableTestItem>)rendered.Instance).Columns
            .Single(c => _testContext.Render(c.Header).Markup == columnTitle);

        await rendered.InvokeAsync(() => column.State!.Visible = visible);
    }
}
