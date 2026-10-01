using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;
using ViciOne.Ui.Localization.Resources;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed class TableFooterTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();
    private readonly TestDragInteraction _fakeDragInteraction = new();

    public TableFooterTests()
    {
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        _testContext.Services.AddScoped<IDragInteraction>(_ => _fakeDragInteraction);
        _testContext.Services.AddAdvancedTable();
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Footer_renders_when_supplied_in_the_footer_fragment()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
        };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .AddFooter());

        // Assert — the count comes from the table, which is what the cascade makes available
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().ContainSingle();
        renderedComponent.Markup.Should().Contain($"2 {CommonVocabulary.ElementPlural}");
    }

    [Fact]
    public void Footer_does_not_render_when_the_footer_fragment_is_unsupplied()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Content") };

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items)));

        // Assert
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().BeEmpty();
    }

    [Fact]
    public void Neither_count_supplied_displays_the_count_the_table_reports()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);

        // Act
        var renderedComponent = RenderFooter(table);

        // Assert
        renderedComponent.Markup.Should().Contain($"3 {CommonVocabulary.ElementPlural}");
    }

    [Fact]
    public void A_single_item_displays_the_singular_noun()
    {
        // Arrange
        var table = CreateTable(itemCount: 1);

        // Act
        var renderedComponent = RenderFooter(table);

        // Assert
        renderedComponent.Markup.Should().Contain($"1 {CommonVocabulary.Element}");
    }

    [Fact]
    public void Supplied_item_count_is_displayed_instead_of_the_count_the_table_reports()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);

        // Act
        var renderedComponent = RenderFooter(table, itemCount: 42);

        // Assert
        renderedComponent.Markup.Should().Contain($"42 {CommonVocabulary.ElementPlural}");
        renderedComponent.Markup.Should().NotContain($"3 {CommonVocabulary.ElementPlural}");
    }

    [Fact]
    public void Supplied_item_count_survives_a_provide()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);
        var renderedComponent = RenderFooter(table, itemCount: 42);
        var renderCount = renderedComponent.RenderCount;

        table.GetTotalItemCount().Returns(7);

        // Act — the table reports a new set of items
        table.ItemsChanged += Raise.Event<Action>();

        // Assert — the supplied count is neither replaced nor re-read, and the notification costs no re-render
        renderedComponent.Markup.Should().Contain($"42 {CommonVocabulary.ElementPlural}");
        renderedComponent.RenderCount.Should().Be(renderCount);
        table.DidNotReceive().GetTotalItemCount();
    }

    [Fact]
    public void Item_count_returning_to_unsupplied_displays_the_count_the_table_reports_now()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);
        var renderedComponent = RenderFooter(table, itemCount: 42);

        table.GetTotalItemCount().Returns(7);
        table.ItemsChanged += Raise.Event<Action>();

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ItemCount, null));

        // Assert — the count the provide was not allowed to cache is read now, rather than a stale one
        renderedComponent.Markup.Should().Contain($"7 {CommonVocabulary.ElementPlural}");
    }

    [Fact]
    public void Another_parameter_changing_does_not_re_read_the_count_the_table_reports()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);
        var renderedComponent = RenderFooter(table);

        table.ClearReceivedCalls();

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.SelectedItemCount, 2));

        // Assert — the count is kept current by the table's notification, not by parameter updates
        table.DidNotReceive().GetTotalItemCount();
        renderedComponent.Markup.Should().Contain($"3 {CommonVocabulary.ElementPlural}");
    }

    [Fact]
    public void Unsupplied_and_zero_selection_count_display_the_same_text()
    {
        // Arrange
        var unsuppliedTable = CreateTable(itemCount: 3);
        var zeroTable = CreateTable(itemCount: 3);

        // Act
        var withUnsupplied = RenderFooter(unsuppliedTable);
        var withZero = RenderFooter(zeroTable, selectedItemCount: 0);

        // Assert
        withZero.Markup.Should().Be(withUnsupplied.Markup);
    }

    [Fact]
    public void Supplied_selection_count_is_displayed_beside_the_item_count()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);

        // Act
        var renderedComponent = RenderFooter(table, selectedItemCount: 2);

        // Assert
        renderedComponent.Markup.Should()
            .Contain($"3 {CommonVocabulary.ElementPlural} | 2 {CommonVocabulary.Selected}");
    }

    [Fact]
    public void Both_counts_supplied_displays_exactly_what_was_supplied()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);

        // Act
        var renderedComponent = RenderFooter(table, itemCount: 42, selectedItemCount: 2);

        // Assert
        renderedComponent.Markup.Should()
            .Contain($"42 {CommonVocabulary.ElementPlural} | 2 {CommonVocabulary.Selected}");
    }

    [Fact]
    public void A_disposed_footer_stops_reading_the_count_the_table_reports()
    {
        // Arrange
        var table = CreateTable(itemCount: 3);
        var renderedComponent = RenderFooter(table);

        renderedComponent.Instance.Dispose();
        table.ClearReceivedCalls();

        // Act — the table reports a new set of items after the footer is gone
        table.ItemsChanged += Raise.Event<Action>();

        // Assert — the handler is unsubscribed, so it never reaches StateHasChanged on a disposed component
        table.DidNotReceive().GetTotalItemCount();
    }

    private static IAdvancedTable<TableTestItem> CreateTable(int itemCount)
    {
        var table = Substitute.For<IAdvancedTable<TableTestItem>>();
        table.GetTotalItemCount().Returns(itemCount);

        return table;
    }

    // Renders the footer against a substituted table instead of a real one, so a test can decide what the
    // table reports and when it reports it.
    private IRenderedComponent<TableFooter<TableTestItem>> RenderFooter(
        IAdvancedTable<TableTestItem> table, int? itemCount = null, int? selectedItemCount = null)
            => _testContext.Render<TableFooter<TableTestItem>>(b => b
                .AddCascadingValue(table)
                .Add(p => p.ItemCount, itemCount)
                .Add(p => p.SelectedItemCount, selectedItemCount));
}
