using Bunit;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ContextMenuComponent = ViciOne.Ui.Blazor.Components.ContextMenu.Components.ContextMenu;

namespace ViciOne.Ui.Blazor.Components.Tests.ContextMenu.Components;

public sealed class ContextMenuTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(ContextMenuComponent).Assembly.GetName().Name}/context-menu/components/context-menu.js";

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddPopup();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        var renderedComponent = testContext.Render<ContextMenuComponent>();

        // The module is loaded the first time the menu is shown.
        await renderedComponent.Instance.ShowAsync(new MouseEventArgs(), null);
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
