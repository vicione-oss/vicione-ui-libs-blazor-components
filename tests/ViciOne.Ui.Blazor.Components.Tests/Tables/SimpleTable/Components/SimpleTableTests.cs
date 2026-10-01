using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();
    private readonly TestDragInteraction _fakeDragInteraction = new();

    public SimpleTableTests()
    {
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        _testContext.Services.AddScoped<IDragInteraction>(_ => _fakeDragInteraction);
        _testContext.Services.AddSimpleTable().AddLogging();
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Should_render_without_items_and_without_any_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>();

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items]));

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Fact]
    public void Should_render_the_css_class_on_the_root_element()
    {
        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.CssClass, "custom"));

        // Assert
        renderedComponent.Find(".advanced-table").ClassList.Should().Contain("custom");
    }

    [Fact]
    public void Setting_a_filter_that_is_not_applicable_in_memory_does_not_throw_and_is_skipped()
    {
        // Arrange — TestColumnFilter is an IColumnFilter but not an ISimpleTableColumnFilter<TItem>, so
        // SimpleTable cannot apply it in-memory. It must be logged-and-skipped, never thrown: a throw on the
        // user's Apply click would kill the Blazor circuit.
        var items = new List<TableTestItem> { new(1, "Content One"), new(2, "Content Two") };
        var filter = FilterState.Empty.WithColumnFilter(new TestColumnFilter("TestValue"));

        // Act — the column has to exist, or the filter is dropped before the provider ever sees its type
        var act = () => _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.FilterState, filter)
            .AddColumns());

        // Assert — no throw; the skipped filter leaves every row visible
        var rendered = act.Should().NotThrow().Subject;
        rendered.FindAll("tbody tr").Should().HaveCount(2);
    }

    [Fact]
    public void Filter_state_parameter_is_applied_on_first_render()
    {
        // Arrange — the column only registers while the table first renders; the filter must still reach the
        // first painted rows
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };
        var filter = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", "ban", x => x.TestValue));

        // Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.FilterState, filter)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.CloseComponent();
            }));

        // Assert — only the matching row survives on first paint
        rendered.WaitForAssertion(() => rendered.FindAll("tbody tr").Should().ContainSingle());
    }

    [Fact]
    public void Sorting_state_parameter_is_applied_on_first_render()
    {
        // Arrange — the first-paint guarantee of the filter state parameter, for sorting
        var items = new List<TableTestItem> { new(3, "Cherry"), new(1, "Apple"), new(2, "Banana") };

        // Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .Add(p => p.SortingState, SortingState.Empty.WithColumnSorting("TestValue", ascending: true))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.SortExpression),
                    (Expression<Func<TableTestItem, object>>)(x => x.TestValue));
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.CloseComponent();
            }));

        // Assert — rows are sorted alphabetically on first paint
        rendered.WaitForAssertion(() =>
        {
            var cells = rendered.FindComponents<CellContainer>().Skip(1).ToList();
            cells[0].Markup.Should().Contain("Apple");
            cells[1].Markup.Should().Contain("Banana");
            cells[2].Markup.Should().Contain("Cherry");
        });
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
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items]));

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<SimpleTableSelectColumn<TableTestItem>>().Should().BeEmpty();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
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
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns()
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<SimpleTableSelectColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
    }

    [Fact]
    public void Should_render_with_all_child_components()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns()
            .AddFooter()
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<SimpleTableSelectColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public void Unsupplied_footer_emits_no_footer_element()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Content One") };

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns());

        // Assert
        renderedComponent.FindAll(".footer").Should().BeEmpty();
    }

    [Fact]
    public void Supplied_footer_emits_a_footer_element()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Content One") };

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns()
            .AddFooter());

        // Assert
        renderedComponent.FindAll(".footer").Should().ContainSingle();
    }

    [Fact]
    public void Header_slot_renders_its_content()
    {
        // Arrange
        List<TableTestItem> items = [];

        const string HeaderText = "My Table Header";

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Header, headerBuilder => headerBuilder.AddContent(0, HeaderText))
        );

        // Assert
        renderedComponent.Find(".header").TextContent.Should().Contain(HeaderText);
    }

    [Fact]
    public void LeftOutlet_slot_renders_its_content()
    {
        // Arrange
        List<TableTestItem> items = [];

        const string PaneText = "Left panel content";

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.LeftOutlet, paneBuilder => paneBuilder.AddContent(0, PaneText))
        );

        // Assert
        renderedComponent.Find(".left-outlet").TextContent.Should().Contain(PaneText);
    }

    [Fact]
    public void Changing_item_source_reference_updates_displayed_data()
    {
        // Arrange
        List<TableTestItem> firstItems = [new(1, "First")];
        List<TableTestItem> secondItems = [new(2, "Second")];

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, firstItems)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.CloseComponent();
            })
        );

        renderedComponent.Markup.Should().Contain("First");

        // Act — provide a new reference
        renderedComponent.Render(b => b
            .Add(p => p.Items, secondItems)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.Markup.Should().Contain("Second");
        renderedComponent.Markup.Should().NotContain("First");
    }

    [Fact]
    public void Rows_draggable_false_does_not_attach_row_to_drag_interaction()
    {
        // Arrange & Act
        _ = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [new(1, "Value")])
            .Add(p => p.RowsDraggable, false));

        // Assert: drag is JS-pointer-based, so a non-draggable row is never attached to the interaction
        _fakeDragInteraction.LastAttached.Should().BeNull();
    }

    [Fact]
    public void Rows_draggable_true_attaches_row_to_drag_interaction()
    {
        // Arrange & Act
        _ = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [new(1, "Value")])
            .Add(p => p.RowsDraggable, true));

        // Assert: drag is JS-pointer-based, so a draggable row attaches itself to the interaction
        _fakeDragInteraction.LastAttached.Should().NotBeNull();
        _fakeDragInteraction.LastAttached!.Draggable.Should().BeTrue();
    }

    [Fact]
    public async Task Clicking_row_with_single_selection_fires_callback()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item])
            .Add(p => p.SelectionMode, SelectionMode.Single)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await renderedComponent.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item]);
    }

    [Fact]
    public async Task Default_selection_mode_is_multiple_so_clicking_a_row_selects_it()
    {
        // Arrange: no SelectionMode set — the default flips to Multiple, forwarded to the inner AdvancedTable
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item])
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await renderedComponent.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeEquivalentTo([item]);
    }

    [Fact]
    public async Task RowClickSelectionEnabled_false_is_forwarded_so_row_click_does_not_select()
    {
        // Arrange: the new parameter must reach the inner AdvancedTable's row-click channel
        var item = new TableTestItem(1, "Value");
        List<TableTestItem>? received = null;

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item])
            .Add(p => p.SelectionMode, SelectionMode.Multiple)
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.SelectedItemsChanged, EventCallback.Factory.Create<List<TableTestItem>>(this, l => received = l)));

        // Act
        await renderedComponent.Find("tbody tr").ClickAsync(new MouseEventArgs());

        // Assert
        received.Should().BeNull();
    }
}
