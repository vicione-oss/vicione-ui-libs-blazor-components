using Bunit;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public void Empty_items_render_the_no_data_placeholder()
    {
        // Arrange & Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .AddColumns());

        // Assert
        renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle()
            .Which.TextContent.Should().Be("No data");
    }

    [Fact]
    public async Task Filtering_every_row_away_renders_the_no_data_placeholder()
    {
        // Arrange
        var items = new List<TableTestItem> { new(1, "Apple"), new(2, "Banana") };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [.. items])
            .AddColumns());

        renderedComponent.FindAll("tbody tr.no-data").Should().BeEmpty();

        // Act
        var filter = FilterState.Empty.WithColumnFilter(
            new SimpleTableContainsColumnFilter<TableTestItem>("TestValue", "nothing matches this",
                item => item.TestValue));

        await renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetFilterStateAsync(filter));

        // Assert
        renderedComponent.WaitForAssertion(() =>
            renderedComponent.FindAll("tbody tr.no-data").Should().ContainSingle());
    }
}
