using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using Xunit;
using TestContext = Bunit.TestContext;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizing.Service;

public class ResizeObserverTests
{
    [Fact]
    public async Task On_ObserveAsync_should_invoke_observe_if_not_disposed()
    {
        // Arrange
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        // Act
        await sut.ObserveAsync(new ElementReference());

        // Assert
        ctx.JSInterop.VerifyInvoke("observe");
    }

    [Fact]
    public async Task On_ObserveAsync_should_not_invoke_observe_if_disposed()
    {
        // Arrange
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        // Act
        await sut.DisposeAsync();
        await sut.ObserveAsync(new ElementReference());

        // Assert
        ctx.JSInterop.VerifyNotInvoke("observe");
    }

    [Fact]
    public async Task On_UnobserveAsync_should_invoke_observe_if_not_disposed()
    {
        // Arrange
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        // Act
        await sut.UnobserveAsync(new ElementReference());

        // Assert
        ctx.JSInterop.VerifyInvoke("unobserve");
    }

    [Fact]
    public async Task On_UnobserveAsync_should_not_invoke_observe_if_disposed()
    {
        // Arrange
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        // Act
        await sut.DisposeAsync();
        await sut.UnobserveAsync(new ElementReference());

        // Assert
        ctx.JSInterop.VerifyNotInvoke("unobserve");
    }
}
