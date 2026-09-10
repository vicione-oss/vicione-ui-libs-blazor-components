using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class ShowAsync : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public ShowAsync()
            => _testContext.Services.AddPopup();

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_make_popup_visible()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.CssClass, "test-popup"));

            // Act
            await popup.Instance.ShowAsync();

            // Assert
            var assert = () =>
            {
                var sectionContent = popup.RenderSectionContent(_testContext);

                sectionContent.Find(".test-popup");
            };

            assert.Should().NotThrow();
        }

        [Fact]
        public async Task Should_render_tabindex_on_modal_dialog()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>();

            // Act
            await popup.Instance.ShowAsync();

            // Assert
            var sectionContent = popup.RenderSectionContent(_testContext);

            sectionContent.Find(".modal-dialog").GetAttribute("tabindex").Should().Be("-1");
        }

        [Fact]
        public async Task Should_focus_modal_dialog()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>();

            await popup.Instance.ShowAsync();

            // Act
            popup.RenderSectionContent(_testContext);
            popup.Render(b => b.Add(p => p.Visible, true));

            // Assert
            var assert = () => _testContext.JSInterop.VerifyFocusAsyncInvoke();

            assert.Should().NotThrow();
        }

        [Fact]
        public async Task Should_invoke_on_showing()
        {
            // Arrange
            var onShowingInvoked = false;

            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.OnShowing, () => onShowingInvoked = true));

            // Act
            await popup.Instance.ShowAsync();

            // Assert
            onShowingInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task Should_invoke_visible_changed_with_true()
        {
            // Arrange
            bool? visibleChangedValue = null;

            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.VisibleChanged, value => visibleChangedValue = value));

            // Act
            await popup.Instance.ShowAsync();

            // Assert
            visibleChangedValue.Should().BeTrue();
        }

        [Fact]
        public async Task Should_not_invoke_on_showing_when_already_visible()
        {
            // Arrange
            var onShowingCallCount = 0;

            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.OnShowing, () => onShowingCallCount++));

            await popup.Instance.ShowAsync();

            // Act
            await popup.Instance.ShowAsync();

            // Assert
            onShowingCallCount.Should().Be(1);
        }

        [Fact]
        public async Task Should_not_throw_when_disposed()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>();

            await popup.Instance.DisposeAsync();

            // Act
            var showAction = async () => popup.Instance.ShowAsync();

            // Assert
            await showAction.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Concurrent_calls_should_invoke_on_showing_once()
        {
            // Arrange
            var onShowingCallCount = 0;

            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.OnShowing, () => onShowingCallCount++));

            // Act
            await Task.WhenAll(
                popup.Instance.ShowAsync(),
                popup.Instance.ShowAsync(),
                popup.Instance.ShowAsync());

            // Assert
            onShowingCallCount.Should().Be(1);
        }
    }
}
