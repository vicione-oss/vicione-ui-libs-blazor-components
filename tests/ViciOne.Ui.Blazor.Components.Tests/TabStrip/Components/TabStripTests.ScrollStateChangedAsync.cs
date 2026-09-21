using Bunit;
using TabStripComponent = ViciOne.Ui.Blazor.Components.TabStrip.Components.TabStrip;

namespace ViciOne.Ui.Blazor.Components.Tests.TabStrip.Components;

public sealed partial class TabStripTests
{
    public sealed class ScrollStateChangedAsync : IAsyncDisposable
    {
        private readonly BunitContext _testContext;

        public ScrollStateChangedAsync()
        {
            _testContext = new BunitContext();

            var jsModule = _testContext.JSInterop.SetupModule(s_jsModuleIdentifier);
            var jsInstance = jsModule.SetupModule("TabStrip", _ => true);
            jsInstance.Mode = JSRuntimeMode.Loose;
        }

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_re_render_only_when_state_changes()
        {
            // Arrange
            var renderedComponent = _testContext.Render<TabStripComponent>(b => b
                .Add(p => p.ChildContent, _ => { }));

            await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: true, canScrollRight: true);

            var renderCountAfterFirstChange = renderedComponent.RenderCount;

            // Act
            await renderedComponent.Instance.ScrollStateChangedAsync(canScrollLeft: true, canScrollRight: true);

            // Assert
            renderedComponent.RenderCount.Should().Be(renderCountAfterFirstChange);
        }

        [Fact]
        public async Task Should_not_throw_for_repeated_invocations()
        {
            // Arrange
            var renderedComponent = _testContext.Render<TabStripComponent>(b => b
                .Add(p => p.ChildContent, _ => { }));

            // Act
            var act = async () =>
            {
                await renderedComponent.Instance.ScrollStateChangedAsync(true, false);
                await renderedComponent.Instance.ScrollStateChangedAsync(false, true);
                await renderedComponent.Instance.ScrollStateChangedAsync(true, true);
                await renderedComponent.Instance.ScrollStateChangedAsync(false, false);
            };

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
