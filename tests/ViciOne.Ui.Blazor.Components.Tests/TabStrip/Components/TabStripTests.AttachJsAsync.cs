using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using TabStripComponent = ViciOne.Ui.Blazor.Components.TabStrip.Components.TabStrip;

namespace ViciOne.Ui.Blazor.Components.Tests.TabStrip.Components;

public sealed partial class TabStripTests
{
    public sealed class AttachJsAsync : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_not_throw_when_js_constructor_fails()
        {
            // Arrange
            // Switching tabs quickly can remove the tab strip from the DOM before its JS object is created,
            // so the constructor receives a null scroll container and throws.
            await using var jsModule = new ConstructorThrowingJSObjectReference(
                new JSException("Cannot read properties of null (reading 'parentElement')"));

            _testContext.Services.AddSingleton<IJSRuntime>(new ImportingJSRuntime(jsModule));

            // Act
            var act = () => _testContext.Render<TabStripComponent>(b => b
                .Add(p => p.ChildContent, _ => { }));

            // Assert
            act.Should().NotThrow();
            jsModule.ConstructorInvocations.Should().ContainSingle()
                .Which.Identifier.Should().Be("TabStrip");
        }

        private sealed class ImportingJSRuntime(IJSObjectReference jsModule) : IJSRuntime
        {
            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
                => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            {
                if (identifier == "import" && (args?[0] as string) == s_jsModuleIdentifier)
                    return ValueTask.FromResult((TValue)jsModule);

                return ValueTask.FromResult<TValue>(default!);
            }
        }
    }
}
