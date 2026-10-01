using System.Globalization;
using Bunit;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    [Fact]
    public void Empty_item_set_renders_the_no_data_placeholder()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .AddColumns());

        // Assert
        renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle()
            .Which.TextContent.Should().Be("No data");
    }

    [Fact]
    public void No_data_placeholder_spans_every_visible_column()
    {
        // Arrange & Act — AddColumns registers a data column plus a navigation column
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .AddColumns());

        // Assert
        renderedComponent.Find("tbody tr.no-data td").GetAttribute("colspan").Should().Be("2");
    }

    [Fact]
    public void No_data_placeholder_renders_alongside_the_footer()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([]))
            .AddColumns()
            .AddFooter());

        // Assert
        renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle();
        renderedComponent.FindComponents<TableFooter<TableTestItem>>().Should().ContainSingle();
    }

    [Fact]
    public void Non_empty_item_set_renders_no_placeholder()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .AddColumns());

        // Assert
        renderedComponent.FindAll("tbody tr.no-data").Should().BeEmpty();
    }

    [Fact]
    public void Emptying_the_item_set_adds_the_placeholder()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .AddColumns());

        renderedComponent.FindAll("tbody tr.no-data").Should().BeEmpty();

        // Act
        renderedComponent.Render(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([])));

        // Assert
        renderedComponent.WaitForAssertion(() =>
            renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle());
    }

    [Fact]
    public void Placeholder_is_withheld_until_the_provider_has_answered()
    {
        // Arrange — the table renders while the provider is still pending, and an empty body at that point
        // means "loading", not "no data"
        var provider = new GatedItemsProvider([]);

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, provider)
            .AddColumns());

        renderedComponent.FindAll("tbody tr.no-data").Should().BeEmpty();

        // Act
        provider.Release();

        // Assert
        renderedComponent.WaitForAssertion(() =>
            renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle());
    }

    [Fact]
    public void Placeholder_is_not_rendered_when_the_provider_reports_items_beyond_the_current_page()
    {
        // Arrange & Act — the body is empty but the provider reports a non-zero total, so the footer counts
        // rows that exist and the placeholder would contradict it
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new RecordingItemsProvider([], totalItemCount: 100))
            .AddColumns());

        // Assert
        renderedComponent.FindAll("tbody tr.no-data").Should().BeEmpty();
    }

    [Fact]
    public void No_data_placeholder_is_localized_for_the_current_culture()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");

        try
        {
            // Act
            var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
                .Add(p => p.ItemsProvider, new TestItemsProvider([]))
                .AddColumns());

            // Assert
            renderedComponent.Find("tbody tr.no-data").TextContent.Should().Be("Keine Daten");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }
}
