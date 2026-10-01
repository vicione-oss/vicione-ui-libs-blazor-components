using Bunit;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public async Task Right_clicking_a_row_invokes_callback_with_item_and_mouse_when_handler_is_set()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        var received = new List<RowContextMenuEventArgs<TableTestItem>>();
        var args = new MouseEventArgs { ClientX = 42, ClientY = 99 };

        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [item])
            .Add(p => p.RowContextMenuRequested, eventArgs => received.Add(eventArgs)));

        // Act
        await renderedComponent.Find("tbody tr").ContextMenuAsync(args);

        // Assert
        received.Should().ContainSingle();
        received[0].Item.Should().BeSameAs(item);
        received[0].MouseEventArgs.ClientX.Should().Be(args.ClientX);
        received[0].MouseEventArgs.ClientY.Should().Be(args.ClientY);
    }
}
