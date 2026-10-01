using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public void Shift_clicking_a_second_header_orders_the_rows_tied_on_the_first()
    {
        // Arrange: every row ties with one other on TestValue, and their source order is the reverse of their
        // TestKey order, so a sorting by TestValue alone cannot produce the expected rows
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [new(3, "Alpha"), new(4, "Beta"), new(2, "Alpha"), new(1, "Beta")])
            .Add(p => p.RowClickSelectionEnabled, false)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                builder.AddComponentParameter(
                    2,
                    nameof(SimpleTableTemplateColumn<>.SortExpression),
                    (Expression<Func<TableTestItem, object>>)(item => item.TestValue));
                static RenderFragment ValueContent(TableTestItem item)
                    => contentBuilder => contentBuilder.AddContent(0, item.TestValue);
                builder.AddComponentParameter(
                    3,
                    nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)ValueContent);
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(4);
                builder.AddComponentParameter(5, nameof(SimpleTableTemplateColumn<>.Id), "TestKey");
                builder.AddComponentParameter(
                    6,
                    nameof(SimpleTableTemplateColumn<>.SortExpression),
                    (Expression<Func<TableTestItem, object>>)(item => item.TestKey));
                static RenderFragment KeyContent(TableTestItem item)
                    => contentBuilder => contentBuilder.AddContent(
                        0, item.TestKey.ToString(CultureInfo.InvariantCulture));
                builder.AddComponentParameter(
                    7,
                    nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)KeyContent);
                builder.CloseComponent();
            }));

        // Act
        rendered.FindAll(".sortable")[0].Click();
        rendered.WaitForAssertion(() => GetRenderedKeys(rendered).Should().Equal(["3", "2", "4", "1"]));

        rendered.FindAll(".sortable")[1].Click(new MouseEventArgs { ShiftKey = true });

        // Assert
        rendered.WaitForAssertion(() => GetRenderedKeys(rendered).Should().Equal(["2", "3", "1", "4"]));
    }

    private static List<string> GetRenderedKeys(IRenderedComponent<SimpleTable<TableTestItem>> rendered)
        => [.. rendered.FindAll("tbody tr td:last-child").Select(cell => cell.TextContent.Trim())];
}
