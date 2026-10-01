using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public void Pin_side_on_template_column_forwards_through_wrapper_to_rendered_cell()
    {
        // Arrange & Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "pinned");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.PinSide), PinSide.Left);
                builder.CloseComponent();
            }));

        // Assert
        var header = rendered.Find("th");
        header.ClassList.Should().Contain("pinned-column");
        header.GetAttribute("style").Should()
            .Contain("left: var(--pin-left-0)");
    }
}
