using Bunit;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using TextBoxComponent = ViciOne.Ui.Blazor.Components.TextBox.TextBox;

namespace ViciOne.Ui.Blazor.Components.Tests.TextBox;

public sealed class TextBoxTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(TextBoxComponent).Assembly.GetName().Name}/text-box/text-box.js";

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        testContext.Render<TextBoxComponent>();
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
