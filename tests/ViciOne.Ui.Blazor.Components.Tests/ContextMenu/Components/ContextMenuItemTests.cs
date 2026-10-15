using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ContextMenuComponent = ViciOne.Ui.Blazor.Components.ContextMenu.Components.ContextMenu;

namespace ViciOne.Ui.Blazor.Components.Tests.ContextMenu.Components;

public sealed class ContextMenuItemTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(ContextMenuComponent).Assembly.GetName().Name}/context-menu/components/context-menu-item.js";

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddPopup();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        // Only an item with a child menu loads the module.
        testContext.Render<CascadingValue<IContextMenu>>(b => b
            .Add(p => p.Name, Constants.CascadingParameterNames.ParentContextMenu)
            .Add(p => p.Value, Substitute.For<IContextMenu>())
            .Add(p => p.IsFixed, true)
            .AddChildContent<ContextMenuItem>(b => b
                .Add(p => p.Text, "Item")
                .Add(p => p.ChildContent, _ => _ => { })));
        await pendingImport.WaitUntilRequestedAsync();

        // Act
        await testContext.DisposeComponentsAsync();
        await pendingImport.CompleteAsync();

        // Assert
        await pendingImport.Module.Received(1).DisposeAsync();
        await pendingImport.Module.DidNotReceive()
            .InvokeConstructorAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>());
    }
}
