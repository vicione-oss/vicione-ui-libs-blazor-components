using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Columns;

public sealed class TableNavigationColumnTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public TableNavigationColumnTests()
    {
        _testContext.Services.AddAdvancedTable();
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Shows_column_without_cells_for_empty_items()
    {
        // Arrange
        var items = new List<TableTestItem>();

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<TableNavigationColumn<TableTestItem>>(0);
                    builder.CloseComponent();
                })
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableNavigationColumnBodyCellContent<TableTestItem>>().Should().BeEmpty();
    }

    [Fact]
    public void Shows_column_with_cells_for_items()
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
            .Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<TableNavigationColumn<TableTestItem>>(0);
                    builder.CloseComponent();
                })
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<TableNavigationColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.FindComponents<TableNavigationColumnBodyCellContent<TableTestItem>>().Should().HaveCount(items.Count);

        var visibleIcons = renderedComponent.FindAll(".navigate-button--visible");
        visibleIcons.Should().BeEmpty();
    }

    [Fact]
    public void Throws_event_on_click()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        TableTestItem? clickedItem = null;
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<TableNavigationColumn<TableTestItem>>(0);
                    {
                        builder.AddAttribute(1, nameof(TableNavigationColumn<>.Navigate),
                            EventCallback.Factory.Create<TableTestItem>(this, item => clickedItem = item));
                    }
                    builder.CloseComponent();
                })
        );

        var button = renderedComponent.Find(".navigate-button");
        button.Click();

        renderedComponent.WaitForState(() => clickedItem is not null);

        // Assert
        clickedItem.Should().BeEquivalentTo(items[0]);
    }
}
