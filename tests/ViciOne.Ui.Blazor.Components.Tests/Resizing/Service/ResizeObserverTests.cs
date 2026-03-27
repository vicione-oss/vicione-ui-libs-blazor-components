using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizing.Service;

public class ResizeObserverTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ObserveAsync_should_invoke_observe_based_on_disposed_state(bool isDisposed, bool shouldInvoke)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        if (isDisposed)
            await sut.DisposeAsync();

        // Act
        await sut.ObserveAsync(new ElementReference("id"));

        // Assert
        if (shouldInvoke)
            ctx.JSInterop.VerifyInvoke("observe");
        else
            ctx.JSInterop.VerifyNotInvoke("observe");
    }

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(false, true, 0)]
    [InlineData(true, true, 1)]
    public async Task UnobserveAsync_should_invoke_unobserve_based_on_state(bool isObserved, bool isDisposed, int expectedInvokeCount)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        var elementReference = new ElementReference("id");

        if (isObserved)
            await sut.ObserveAsync(elementReference);

        if (isDisposed)
            await sut.DisposeAsync();

        // Act
        await sut.UnobserveAsync(elementReference);

        // Assert
        if (expectedInvokeCount > 0)
            ctx.JSInterop.VerifyInvoke("unobserve", expectedInvokeCount);
        else
            ctx.JSInterop.VerifyNotInvoke("unobserve");
    }

    [Theory]
    [InlineData(true, 50)]
    [InlineData(false, 1)]
    public async Task Concurrent_ObserveAsync_should_invoke_observe_expected_times(bool useUniqueElements, int expectedInvokeCount)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        ElementReference[] elements = useUniqueElements
            ? [.. Enumerable.Range(0, 50).Select(i => new ElementReference($"element-{i}"))]
            : [.. Enumerable.Repeat(new ElementReference("same-element"), 50)];

        // Act
        await Task.WhenAll(elements.Select(e => Task.Run(() => sut.ObserveAsync(e))));

        // Assert
        ctx.JSInterop.VerifyInvoke("observe", expectedInvokeCount);
    }

    [Fact]
    public async Task Concurrent_UnobserveAsync_should_unobserve_all_elements()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        var elements = Enumerable.Range(0, 50)
            .Select(i => new ElementReference($"element-{i}"))
            .ToArray();

        foreach (var element in elements)
            await sut.ObserveAsync(element);

        // Act
        await Task.WhenAll(elements.Select(e => Task.Run(() => sut.UnobserveAsync(e))));

        // Assert
        ctx.JSInterop.VerifyInvoke("unobserve", 50);
    }

    [Fact]
    public async Task Concurrent_ObserveAsync_and_UnobserveAsync_should_not_throw()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        var elementsToObserve = Enumerable.Range(0, 25)
            .Select(i => new ElementReference($"observe-{i}"))
            .ToArray();

        var elementsToUnobserve = Enumerable.Range(0, 25)
            .Select(i => new ElementReference($"unobserve-{i}"))
            .ToArray();

        foreach (var element in elementsToUnobserve)
            await sut.ObserveAsync(element);

        // Act
        var observeTasks = elementsToObserve.Select(e => Task.Run(() => sut.ObserveAsync(e)));
        var unobserveTasks = elementsToUnobserve.Select(e => Task.Run(() => sut.UnobserveAsync(e)));

        await Task.WhenAll(observeTasks.Concat(unobserveTasks));

        // Assert
        ctx.JSInterop.VerifyInvoke("observe", 50); // 25 from arrange + 25 from concurrent act
        ctx.JSInterop.VerifyInvoke("unobserve", 25);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Concurrent_DisposeAsync_should_not_throw(bool concurrentObserve)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var jsRuntime = ctx.Services.GetRequiredService<IJSRuntime>();
        await using var sut = new ResizeObserver(jsRuntime, Substitute.For<ILogger<ResizeObserver>>());

        var elements = Enumerable.Range(0, 50)
            .Select(i => new ElementReference($"element-{i}"))
            .ToArray();

        if (!concurrentObserve)
        {
            foreach (var element in elements)
                await sut.ObserveAsync(element);
        }

        var cancellationToken = Xunit.TestContext.Current.CancellationToken;

        // Act
        var tasks = concurrentObserve
            ? elements.Select(e => Task.Run(() => sut.ObserveAsync(e), cancellationToken))
            : elements.Select(e => Task.Run(() => sut.UnobserveAsync(e), cancellationToken));
        var disposeTask = Task.Run(async () => await sut.DisposeAsync(), cancellationToken);

        await Task.WhenAll(tasks.Append(disposeTask));
    }
}
