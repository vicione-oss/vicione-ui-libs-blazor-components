using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Resizing.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizing.Service;

public partial class ResizeObserverTests
{
    public sealed class UnobserveAsync
    {
        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(true, false, 1)]
        [InlineData(false, true, 0)]
        [InlineData(true, true, 1)]
        public async Task Should_invoke_unobserve_based_on_state(bool isObserved, bool isDisposed, int expectedInvokeCount)
        {
            // Arrange
            await using var ctx = new BunitContext();
            ctx.JSInterop.SetupForResizeObserver();

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
    }
}
