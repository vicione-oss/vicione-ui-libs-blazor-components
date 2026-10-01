using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components;

public sealed partial class AdvancedTableTests
{
    // Note: these tests assert the row's ContextMenuPreventDefault and OnContextMenu parameters, not the
    // rendered markup. Blazor applies the `__internal_preventDefault_oncontextmenu` marker at runtime via
    // JavaScript, so it never appears in bUnit's markup and cannot be asserted there. That the marker is
    // wired up at all is covered by RowContextMenuChromeTests in the Playwright suite.

    [Fact]
    public async Task Right_clicking_a_row_invokes_callback_with_item_and_mouse_when_handler_is_set()
    {
        // Arrange
        var item = new TableTestItem(1, "Value");
        var received = new List<RowContextMenuEventArgs<TableTestItem>>();
        var args = new MouseEventArgs { ClientX = 42, ClientY = 99 };

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([item]))
            .Add(p => p.RowContextMenuRequested, eventArgs => received.Add(eventArgs)));

        // Act
        await renderedComponent.Find("tbody tr").ContextMenuAsync(args);

        // Assert
        received.Should().ContainSingle();
        received[0].Item.Should().BeSameAs(item);
        received[0].MouseEventArgs.ClientX.Should().Be(args.ClientX);
        received[0].MouseEventArgs.ClientY.Should().Be(args.ClientY);
    }

    [Fact]
    public void No_context_menu_listener_is_attached_without_a_handler()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")])));

        // Act
        var row = renderedComponent.FindComponent<AdvancedTableRow<TableTestItem>>();

        // Assert
        row.Instance.OnContextMenu.HasDelegate.Should().BeFalse();
    }

    [Fact]
    public void Native_menu_is_suppressed_while_a_handler_is_wired()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowContextMenuRequested, _ => { }));

        // Act
        var row = renderedComponent.FindComponent<AdvancedTableRow<TableTestItem>>();

        // Assert
        row.Instance.ContextMenuPreventDefault.Should().BeTrue();
    }

    [Fact]
    public void Native_menu_is_left_alone_without_a_handler()
    {
        // Arrange
        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")])));

        // Act
        var row = renderedComponent.FindComponent<AdvancedTableRow<TableTestItem>>();

        // Assert
        row.Instance.ContextMenuPreventDefault.Should().BeFalse();
    }

    [Fact]
    public void Native_menu_is_left_alone_while_custom_menus_are_switched_off()
    {
        // Arrange
        _testContext.Services.GetRequiredService<IContextMenuSettings>().UseCustomMenu = false;

        var renderedComponent = _testContext.Render<AdvancedTable<TableTestItem>>(b => b
            .Add(p => p.ItemsProvider, new TestItemsProvider([new(1, "Value")]))
            .Add(p => p.RowContextMenuRequested, _ => { }));

        // Act
        var row = renderedComponent.FindComponent<AdvancedTableRow<TableTestItem>>();

        // Assert
        row.Instance.ContextMenuPreventDefault.Should().BeFalse();
    }
}
