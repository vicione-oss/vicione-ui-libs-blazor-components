using System.Reflection;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Components;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Tooltip.Components;

public sealed class TooltipDisplayTests
{
    private static readonly string s_jsModulePath =
        $"./_content/{typeof(TooltipDisplay).Assembly.GetName().Name}/tooltip/components/tooltip-display.js";

    // Internal, so the test reaches it and its events through reflection.
    private static readonly Type s_tooltipServiceType = typeof(TooltipDisplay).Assembly
        .GetType("ViciOne.Ui.Blazor.Components.Tooltip.Services.TooltipService", throwOnError: true)!;

    [Theory]
    [InlineData("PreRenderingRequested")]
    [InlineData("TooltipsChanged")]
    public async Task Should_unsubscribe_from_tooltip_service_event_when_disposed(string eventName)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddTooltip();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        testContext.Render<TooltipDisplay>();
        var tooltipService = testContext.Services.GetRequiredService(s_tooltipServiceType);

        // Act
        await testContext.DisposeComponentsAsync();

        // Assert
        GetSubscribers(tooltipService, eventName).Should().BeEmpty(
            "the service outlives the display, so a remaining handler would keep the removed display reacting and in memory");
    }

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

    private static Delegate[] GetSubscribers(object tooltipService, string eventName)
    {
        var field = s_tooltipServiceType.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{s_tooltipServiceType.Name} has no event {eventName}.");

        return ((Delegate?)field.GetValue(tooltipService))?.GetInvocationList() ?? [];
    }
}
