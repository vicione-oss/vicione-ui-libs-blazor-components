using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Columns;

public sealed class AdvancedTableColumnTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public AdvancedTableColumnTests()
    {
        _testContext.Services.AddAdvancedTable();
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Shows_header_content_instead_of_title_when_both_are_set()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };
        const string TitleText = "My Title";
        const string HeaderText = "Custom Header";

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                {
                    builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                    builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Title), TitleText);
                    builder.AddComponentParameter(3, nameof(AdvancedTableColumn<>.HeaderContent),
                        (RenderFragment)(b => b
                            .AddContent(0, HeaderText)));
                }
                builder.CloseComponent();
            }));

        // Assert
        var header = renderedComponent.Find("th");

        header.TextContent.Should().Contain(HeaderText);
        header.TextContent.Should().NotContain(TitleText);
    }

    [Fact]
    public void Width_produces_col_with_style()
    {
        // Arrange
        var items = new List<TableTestItem>();
        const int Width = 150;

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                builder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Width), Width);
                builder.CloseComponent();
            }));

        // Assert
        renderedComponent.Find("col").GetAttribute("style").Should().Be("width: 150px");
    }

    [Fact]
    public void Absent_width_produces_col_without_style()
    {
        // Arrange
        var items = new List<TableTestItem>();

        // Act
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                {
                    builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                }
                builder.CloseComponent();
            }));

        // Assert
        renderedComponent.Find("col").GetAttribute("style").Should().BeNull();
    }

    [Fact]
    public void Dispose_unregisters_column_from_table()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Value") };

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider(items))
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                {
                    builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), "col");
                }
                builder.CloseComponent();
            }));

        renderedComponent.FindAll("th").Should().ContainSingle();

        // Act — removing all columns triggers disposal of the registered column
        renderedComponent.Render(b => b
            .Add(p => p.Columns, _ => { }));

        // Assert
        renderedComponent.WaitForAssertion(() => renderedComponent.FindAll("th").Should().BeEmpty());
    }

    [Fact]
    public void Dispose_does_not_call_unregister_when_column_was_not_registered()
    {
        // Arrange — render fails during OnInitialized (Table is null), so _registered stays false
        using var localContext = new BunitContext();

        var act = () => localContext.Render<AdvancedTableColumn<TableTestItem>>(b => b
            .Add(p => p.Id, "col"));

        act.Should().Throw<InvalidOperationException>();

        // Act — dispose must not call Table.UnregisterColumn since Table is null
        var dispose = localContext.Dispose;

        // Assert
        dispose.Should().NotThrow();
    }

    [Fact]
    public void Column_placed_outside_table_throws_with_helpful_message()
    {
        // Act
        var act = () => _testContext.Render<AdvancedTableColumn<TableTestItem>>(
            b => b.Add(p => p.Id, "col"));

        // Assert
        var exception = act.Should().Throw<InvalidOperationException>().Which;

        exception.Message.Should().Contain(nameof(AdvancedTableColumn<>));
        exception.Message.Should().Contain(nameof(AdvancedTable<>));
    }
}
