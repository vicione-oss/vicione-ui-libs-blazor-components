using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.CheckBox;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Popup.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed class ColumnChooserTests : IDisposable
{
    // Hoisted so a markup rename lands in one place. Every assertion used to carry its own copy of the class
    // name, which is why renaming the chooser's markup left this suite querying elements that no longer exist.
    private const string PopupSelector = ".column-chooser-popup";
    private const string ColumnHeaderSelector = ".column-chooser-content > span";
    private const string ToggleButtonCssClass = "test-toggle-button";
    private const string ToggleButtonSelector = $".{ToggleButtonCssClass}";
    private const string OpenToLeftCssClass = "open-to-left";

    // Must match the `::deep` selector in ColumnChooserToggle.razor.scss, which limits PopupHeaderBodyLayout's body to
    // MaximumVisibleRowCount rows. Neither the compiler nor the CSS build checks that selector against the markup.
    private const string LimitedPopupBodySelector = ".popup-header-body-layout > .body";
    private const string ColumnListSelector = ".column-chooser-content";

    private static readonly object s_outletId = new();

    // ColumnChooserToggle renders no button of its own — this stands in for whatever real button a consumer
    // would supply (a ToolbarButton, typically), just enough to open and close the popup under test. Kept a
    // <div> deliberately: ToolbarButton is one too, so a <button> here would test something no consumer does.
    private static readonly RenderFragment<ColumnChooserButtonContext> s_testButton = context => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ToggleButtonCssClass);
        builder.AddAttribute(2, "onclick", context.Toggle);
        builder.CloseElement();
    };

    private readonly BunitContext _testContext = new();

    public ColumnChooserTests()
    {
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        _testContext.Services.AddLogging();
        _testContext.Services.AddScoped<IDragInteraction>(_ => new TestDragInteraction());
        _testContext.Services.AddAdvancedTable();
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Table_without_a_column_chooser_outlet_id_renders_no_column_chooser()
    {
        // Act
        var rendered = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert
        rendered.FindComponents<ColumnChooserContent<TableTestItem>>().Should().BeEmpty();
    }

    [Fact]
    public void Should_open_popup_on_toggle_click_without_columns()
    {
        // Arrange
        var rendered = RenderTableWithToggle();
        rendered.FindAll(PopupSelector).Should().BeEmpty();

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert
        rendered.FindAll(PopupSelector).Should().HaveCount(1);
        rendered.FindComponents<ColumnChooserContent<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public void Should_open_popup_on_toggle_click_with_columns_and_show_relevant_columns()
    {
        // Arrange — the middle column opts out of the chooser via ShowInColumnChooser="false"
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Title1");
            AddColumn(builder, 3, "Title2", showInColumnChooser: false);
            AddColumn(builder, 6, "Title3");
        });

        rendered.FindAll(PopupSelector).Should().BeEmpty();

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — the exact header list rather than substring checks over the whole popup: a column's title
        // doubles as its id in places, so "does not contain Title2" could pass on markup that does list it.
        rendered.FindAll(PopupSelector).Should().HaveCount(1);
        rendered.FindComponents<CheckBox<bool>>().Should().HaveCount(2);
        rendered.FindAll(ColumnHeaderSelector).Select(header => header.TextContent).Should().Equal("Title1", "Title3");
    }

    [Fact]
    public void Should_close_popup_on_close_button_click()
    {
        // Arrange
        var rendered = RenderTableWithToggle();
        rendered.Find(ToggleButtonSelector).Click();
        rendered.FindAll(PopupSelector).Should().HaveCount(1);

        // Act — the close control belongs to the shared popup chrome now, so it is reached by component rather
        // than by class: the chooser only supplies the IPopup implementation the button calls back into.
        rendered.FindComponent<PopupHeaderCloseActionButton>().Find("button").Click();

        // Assert
        rendered.FindAll(PopupSelector).Should().BeEmpty();
    }

    [Fact]
    public void Should_hide_column_when_unchecked()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Title1");
            AddColumn(builder, 3, "Title2", showInColumnChooser: false);
        });

        rendered.FindAll("th").Should().HaveCount(2);
        rendered.Find(ToggleButtonSelector).Click();

        // Act
        var checkbox = rendered.FindComponents<CheckBox<bool>>().Single().Find("input[type='checkbox']");
        checkbox.Input(false);

        // Assert
        rendered.FindAll("th").Should().HaveCount(1);
    }

    [Fact]
    public void Should_hide_last_column_when_unchecked()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder => AddColumn(builder, 0, "Title1"));

        rendered.FindAll("th").Should().HaveCount(1);
        rendered.Find(ToggleButtonSelector).Click();

        // Act
        var checkbox = rendered.FindComponents<CheckBox<bool>>().Single().Find("input[type='checkbox']");
        checkbox.Input(false);

        // Assert
        rendered.FindAll("th").Should().HaveCount(0);
    }

    [Fact]
    public void Should_raise_visible_changed_when_toggled()
    {
        // Arrange — the callback stands in for the @bind-Visible a consumer writes on the column
        var visibleChanges = new List<bool>();
        var visibleChanged = EventCallback.Factory.Create<bool>(this, visibleChanges.Add);

        var rendered = RenderTableWithToggle(builder => AddColumn(builder, 0, "Title1", visibleChanged: visibleChanged));

        rendered.Find(ToggleButtonSelector).Click();

        // Act — uncheck, then check again, so the round trip a two-way binding needs is covered
        rendered.FindComponents<CheckBox<bool>>().Single().Find("input[type='checkbox']").Input(false);
        rendered.FindComponents<CheckBox<bool>>().Single().Find("input[type='checkbox']").Input(true);

        // Assert
        visibleChanges.Should().Equal([false, true]);
    }

    [Fact]
    public void Should_open_popup_to_the_right_by_default()
    {
        // Arrange
        var rendered = RenderTableWithToggle();

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — right is the absence of the modifier, so the popup keeps the left edge of the button
        rendered.Find(PopupSelector).ClassList.Should().NotContain(OpenToLeftCssClass);
    }

    [Fact]
    public void Should_open_popup_to_the_left_when_requested()
    {
        // Arrange
        var rendered = RenderTableWithToggle(openPopupToLeft: true);

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert
        rendered.Find(PopupSelector).ClassList.Should().Contain(OpenToLeftCssClass);
    }

    [Fact]
    public void Should_limit_the_popup_to_ten_visible_rows_by_default()
    {
        // Arrange
        var rendered = RenderTableWithToggle();

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — the popup turns this custom property into the max-height of its body
        rendered.Find(PopupSelector).GetAttribute("style")
            .Should().Be("--column-chooser-maximum-visible-row-count: 10");
    }

    [Fact]
    public void Should_limit_the_popup_to_the_requested_number_of_visible_rows()
    {
        // Arrange
        var rendered = RenderTableWithToggle(maximumVisibleRowCount: 3);

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert
        rendered.Find(PopupSelector).GetAttribute("style")
            .Should().Be("--column-chooser-maximum-visible-row-count: 3");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_list_at_least_one_visible_row_when_fewer_are_requested(int maximumVisibleRowCount)
    {
        // Arrange
        var rendered = RenderTableWithToggle(maximumVisibleRowCount: maximumVisibleRowCount);

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — no row at all would leave every column unreachable
        rendered.Find(PopupSelector).GetAttribute("style")
            .Should().Be("--column-chooser-maximum-visible-row-count: 1");
    }

    [Fact]
    public void Should_render_the_column_list_in_the_popup_body_its_styles_limit()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder => AddColumn(builder, 0, "Title1"));

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert — if PopupHeaderBodyLayout renames or rewraps its body, the popup's styles stop matching it and a
        // long column list stretches the popup past its dialog again, with nothing else failing
        rendered.FindAll($"{PopupSelector} {LimitedPopupBodySelector} {ColumnListSelector}").Should().ContainSingle();
    }

    [Fact]
    public void Should_report_popup_visibility_to_the_button()
    {
        // Arrange — the button fragment is the only place ColumnChooserButtonContext is seen, so recording what
        // it is handed is the only way to observe the flag a consumer binds its own pressed state to
        var reportedStates = new List<bool>();

        RenderFragment RecordingButton(ColumnChooserButtonContext context)
            => builder =>
            {
                // The fragment re-renders more often than the flag changes, so only transitions are recorded.
                if (reportedStates.Count == 0 || reportedStates[^1] != context.PopupVisible)
                    reportedStates.Add(context.PopupVisible);

                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", ToggleButtonCssClass);
                builder.AddAttribute(2, "onclick", context.Toggle);
                builder.CloseElement();
            };

        var rendered = RenderTableWithToggle(button: RecordingButton);

        // Act — open through the button, close through the popup's own close control
        rendered.Find(ToggleButtonSelector).Click();
        rendered.FindComponent<PopupHeaderCloseActionButton>().Find("button").Click();

        // Assert
        reportedStates.Should().Equal([false, true, false]);
    }

    [Fact]
    public void Should_list_columns_in_the_tables_render_order_not_the_declaration_order()
    {
        // Arrange
        var rendered = RenderTableWithToggle(builder =>
        {
            AddColumn(builder, 0, "Right", pinSide: PinSide.Right);
            AddColumn(builder, 10, "Middle");
            AddColumn(builder, 20, "Left", pinSide: PinSide.Left);
        });

        // Act
        rendered.Find(ToggleButtonSelector).Click();

        // Assert
        var chooserTitles = rendered.FindAll(ColumnHeaderSelector).Select(header => header.TextContent);
        var headerTitles = rendered.FindAll("th").Select(header => header.TextContent.Trim());

        chooserTitles.Should().Equal("Left", "Middle", "Right");
        chooserTitles.Should().Equal(headerTitles);
    }

    // The button/popup (ColumnChooserToggle) and the table (AdvancedTable, via its ColumnChooserToggleId
    // section) are rendered as siblings in one root, exactly like a real page places them independently of
    // each other — connected only by sharing the same section id, never by component nesting.
    private IRenderedComponent<ContainerFragment> RenderTableWithToggle(RenderFragment? columns = null,
        bool openPopupToLeft = false, RenderFragment<ColumnChooserButtonContext>? button = null,
        int? maximumVisibleRowCount = null)
        => _testContext.Render(builder =>
        {
            builder.OpenComponent<AdvancedTable<TableTestItem>>(0);
            builder.AddComponentParameter(1, nameof(AdvancedTable<>.ItemsProvider), new TestItemsProvider([]));
            builder.AddComponentParameter(2, nameof(AdvancedTable<>.ColumnChooserToggleId), s_outletId);

            if (columns is not null)
                builder.AddComponentParameter(3, nameof(AdvancedTable<>.Columns), columns);

            builder.CloseComponent();

            builder.OpenComponent<ColumnChooserToggle>(4);
            builder.AddComponentParameter(5, nameof(ColumnChooserToggle.Id), s_outletId);
            builder.AddComponentParameter(6, nameof(ColumnChooserToggle.Button), button ?? s_testButton);

            if (openPopupToLeft)
                builder.AddComponentParameter(7, nameof(ColumnChooserToggle.OpenPopupToLeft), true);

            if (maximumVisibleRowCount is { } rowCount)
                builder.AddComponentParameter(8, nameof(ColumnChooserToggle.MaximumVisibleRowCount), rowCount);

            builder.CloseComponent();
        });

    private static void AddColumn(RenderTreeBuilder builder, int sequence, string title,
        bool showInColumnChooser = true, EventCallback<bool> visibleChanged = default,
        PinSide pinSide = PinSide.None)
    {
        builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(sequence);
        builder.AddComponentParameter(sequence + 1, nameof(AdvancedTableColumn<>.Id), Guid.NewGuid().ToString());
        builder.AddComponentParameter(sequence + 2, nameof(AdvancedTableColumn<>.Title), title);
        builder.AddComponentParameter(sequence + 5, nameof(AdvancedTableColumn<>.PinSide), pinSide);

        if (!showInColumnChooser)
            builder.AddComponentParameter(sequence + 3, nameof(AdvancedTableColumn<>.ShowInColumnChooser), false);

        if (visibleChanged.HasDelegate)
            builder.AddComponentParameter(sequence + 4, nameof(AdvancedTableColumn<>.VisibleChanged), visibleChanged);

        builder.CloseComponent();
    }
}
