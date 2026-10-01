using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.CheckBox;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed class SimpleTableColumnChooserTests : IDisposable
{
    // Hoisted so a markup rename lands in one place. Every assertion used to carry its own copy of the class
    // name, which is why renaming the chooser's markup left this suite querying elements that no longer exist.
    private const string ColumnHeaderSelector = ".column-chooser-content > span";
    private const string ColumnHeaderCheckBoxSelector = ".column-chooser-content > span input[type='checkbox']";
    private const string ToggleButtonSelector = ".test-toggle-button";

    private static readonly object s_outletId = new();

    // ColumnChooserToggle renders no button of its own — this stands in for whatever real button a consumer
    // would supply (a ToolbarButton, typically), just enough to open and close the popup under test. Kept a
    // <div> deliberately: ToolbarButton is one too, so a <button> here would test something no consumer does.
    private static readonly RenderFragment<ColumnChooserButtonContext> s_testButton = context => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "test-toggle-button");
        builder.AddAttribute(2, "onclick", context.Toggle);
        builder.CloseElement();
    };

    private readonly BunitContext _testContext = new();

    public SimpleTableColumnChooserTests()
    {
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        _testContext.Services.AddScoped<IDragInteraction>(_ => new TestDragInteraction());
        _testContext.Services.AddSimpleTable().AddLogging();
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Chooser_bound_to_simple_table_lists_the_chooser_eligible_columns()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Title1");
            AddColumn(builder, 3, "Title3");
        });

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert
        rendered.FindComponents<CheckBox<bool>>().Should().HaveCount(2);
        rendered.FindAll(ColumnHeaderSelector).Select(header => header.TextContent).Should().Equal("Title1", "Title3");
    }

    [Fact]
    public void Column_opting_out_is_omitted_from_the_chooser_popup()
    {
        // Arrange — the middle column opts out via ShowInColumnChooser="false"
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Title1");
            AddColumn(builder, 3, "Title2", showInColumnChooser: false);
            AddColumn(builder, 6, "Title3");
        });

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — the exact header list rather than substring checks over the whole popup: a column's title
        // doubles as its id here, so "does not contain Title2" could pass on markup that does list it.
        rendered.FindComponents<CheckBox<bool>>().Should().HaveCount(2);
        rendered.FindAll(ColumnHeaderSelector).Select(header => header.TextContent).Should().Equal("Title1", "Title3");
    }

    [Fact]
    public void Table_without_a_column_chooser_outlet_id_renders_no_column_chooser()
    {
        // Act
        var table = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, []));

        // Assert
        table.FindComponents<CheckBox<bool>>().Should().BeEmpty();
    }

    [Fact]
    public void Toggling_a_column_hides_and_re_shows_it_in_the_rendered_table()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Title1");
            AddColumn(builder, 3, "Title2");
        });

        rendered.FindAll("th").Should().HaveCount(2);

        rendered.Find(ToggleButtonSelector).Click();

        // Act — hide the first column
        rendered.FindComponents<CheckBox<bool>>()[0].Find("input[type='checkbox']").Input(false);

        // Assert — the table drops the column
        rendered.FindAll("th").Should().HaveCount(1);

        // Act — re-show it. Re-found rather than reusing the earlier reference: hiding the column re-rendered
        // the section content, which replaced the checkbox's underlying element.
        rendered.FindComponents<CheckBox<bool>>()[0].Find("input[type='checkbox']").Input(true);

        // Assert — the column comes back
        rendered.FindAll("th").Should().HaveCount(2);
    }

    [Fact]
    public void Chooser_entry_of_a_state_reading_header_stays_operable()
    {
        // Arrange — the select column's header is a live component that pokes the cascaded column state, and
        // the chooser labels its entry with that very fragment. Without the cascade the copy holds no state
        // and the first click takes the circuit down. The column opts into the chooser explicitly: a structural
        // column is left out of it by default.
        var items = new List<TableTestItem> { new(1, "A"), new(2, "B") };
        List<TableTestItem>? selected = null;

        var rendered = RenderTableWithToggle(builder =>
        {
            builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(0);
            builder.AddComponentParameter(1, nameof(SimpleTableSelectColumn<>.ShowInColumnChooser), true);
            builder.CloseComponent();

            AddColumn(builder, 3, "Title1");
        },
        items,
        EventCallback.Factory.Create<List<TableTestItem>>(this, s => selected = s));

        rendered.Find(ToggleButtonSelector).Click();

        // Act — select all through the copy inside the chooser, not through the table's own header
        rendered.Find(ColumnHeaderCheckBoxSelector).Input(true);

        // Assert — it reached the selection instead of throwing
        selected.Should().NotBeNull();
        selected.Should().BeEquivalentTo(items);
    }

    // The toggle (button/popup) and the table are rendered as siblings in one root, exactly like a real page
    // places them independently of each other — connected only by sharing the same section id, never by
    // component nesting.
    private IRenderedComponent<ContainerFragment> RenderTableWithToggle(RenderFragment columns,
        IReadOnlyList<TableTestItem>? items = null,
        EventCallback<List<TableTestItem>> selectedItemsChanged = default)
        => _testContext.Render(builder =>
        {
            builder.OpenComponent<SimpleTable<TableTestItem>>(0);
            builder.AddComponentParameter(1, nameof(SimpleTable<>.Items), items ?? []);
            builder.AddComponentParameter(2, nameof(SimpleTable<>.Columns), columns);
            builder.AddComponentParameter(3, nameof(SimpleTable<>.ColumnChooserToggleId), s_outletId);

            if (selectedItemsChanged.HasDelegate)
                builder.AddComponentParameter(7, nameof(SimpleTable<>.SelectedItemsChanged), selectedItemsChanged);

            builder.CloseComponent();

            builder.OpenComponent<ColumnChooserToggle>(4);
            builder.AddComponentParameter(5, nameof(ColumnChooserToggle.Id), s_outletId);
            builder.AddComponentParameter(6, nameof(ColumnChooserToggle.Button), s_testButton);
            builder.CloseComponent();
        });

    private static void AddColumn(RenderTreeBuilder builder, int sequence, string title, bool showInColumnChooser = true)
    {
        builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(sequence);
        builder.AddComponentParameter(sequence + 1, nameof(SimpleTableTemplateColumn<>.Id), title);
        builder.AddComponentParameter(sequence + 2, nameof(SimpleTableTemplateColumn<>.Title), title);

        if (!showInColumnChooser)
            builder.AddComponentParameter(sequence + 3, nameof(SimpleTableTemplateColumn<>.ShowInColumnChooser), false);

        builder.CloseComponent();
    }
}
