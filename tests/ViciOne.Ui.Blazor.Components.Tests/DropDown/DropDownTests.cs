using Bunit;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using DropDownComponent = ViciOne.Ui.Blazor.Components.DropDown.DropDown<string>;

namespace ViciOne.Ui.Blazor.Components.Tests.DropDown;

public sealed class DropDownTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(DropDownComponent).Assembly.GetName().Name}/drop-down/drop-down.js";

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        testContext.Render<DropDownComponent>(b => b
            .Add(p => p.Items, ["Item"])
            .Add(p => p.ItemTemplate, item => item)
            .Add(p => p.InputElementReference, null));
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
