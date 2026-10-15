using Bunit;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Components;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tooltip.Components;

public sealed class TooltipDisplayTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(TooltipDisplay).Assembly.GetName().Name}/tooltip/components/tooltip-display.js";

    [Fact]
    public async Task Should_dispose_module_without_calling_into_it_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddTooltip();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var pendingImport = PendingJSModuleImport.SetUp(testContext, s_jsModulePath);

        testContext.Render<TooltipDisplay>();
        await pendingImport.WaitUntilRequestedAsync();

        // Act
        await testContext.DisposeComponentsAsync();
        await pendingImport.CompleteAsync();

        // Assert
        await pendingImport.Module.Received(1).DisposeAsync();
        pendingImport.Module.ReceivedCalls().Should().ContainSingle(
            "a display removed while its module was loading has no window left to measure");
    }
}
