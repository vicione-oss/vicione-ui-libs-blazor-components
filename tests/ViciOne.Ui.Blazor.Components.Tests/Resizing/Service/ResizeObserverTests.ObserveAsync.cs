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
    public sealed class ObserveAsync
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task Should_invoke_observe_based_on_disposed_state(bool isDisposed, bool shouldInvoke)
        {
            // Arrange
            await using var ctx = new BunitContext();
            ctx.JSInterop.SetupForResizeObserver();

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
    }
}
