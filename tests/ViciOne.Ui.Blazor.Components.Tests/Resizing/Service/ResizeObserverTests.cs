using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Resizing.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizing.Service;

public partial class ResizeObserverTests
{
    [Theory]
    [InlineData(true, 50)]
    [InlineData(false, 1)]
    public async Task Concurrent_observe_async_should_invoke_observe_expected_times(bool useUniqueElements, int expectedInvokeCount)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.SetupForResizeObserver();

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
    public async Task Concurrent_unobserve_async_should_unobserve_all_elements()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.SetupForResizeObserver();

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
    public async Task Concurrent_observe_async_and_unobserve_async_should_not_throw()
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.SetupForResizeObserver();

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
    public async Task Concurrent_dispose_async_should_not_throw(bool concurrentObserve)
    {
        // Arrange
        await using var ctx = new BunitContext();
        ctx.JSInterop.SetupForResizeObserver();

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
