using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class CloseAsync : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public CloseAsync()
            => _testContext.Services.AddPopup();

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_not_throw_when_disposed()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.Visible, true));

            await popup.Instance.DisposeAsync();

            // Act
            var closeAction = async () => popup.Instance.CloseAsync();

            // Assert
            await closeAction.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Should_make_popup_closed()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CssClass, "test-popup")
                .Add(p => p.Visible, true));

            // Verify popup is initially visible
            var sectionContent = popup.RenderSectionContent(_testContext);
            sectionContent.Find(".test-popup");

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            popup.Markup.Should().BeEmpty();
        }

        [Fact]
        public async Task Should_invoke_on_closing()
        {
            // Arrange
            var onClosingInvoked = false;

            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.Visible, true)
                .Add(p => p.OnClosing, () => onClosingInvoked = true));

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            onClosingInvoked.Should().BeTrue();
        }

        [Fact]
        public async Task Should_invoke_visible_changed_with_false()
        {
            // Arrange
            bool? visibleChangedValue = null;

            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.Visible, true)
                .Add(p => p.VisibleChanged, value => visibleChangedValue = value));

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            visibleChangedValue.Should().BeFalse();
        }

        [Fact]
        public async Task Should_not_invoke_on_closing_when_already_hidden()
        {
            // Arrange
            var onClosingCallCount = 0;

            var popup = _testContext.Render<PopupComponent>(b => b.Add(p => p.OnClosing, () => onClosingCallCount++));

            // Act
            await popup.Instance.CloseAsync();

            // Assert
            onClosingCallCount.Should().Be(0);
        }

        [Fact]
        public async Task Concurrent_calls_should_invoke_on_closing_once()
        {
            // Arrange
            var onClosingCallCount = 0;

            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.Visible, true)
                .Add(p => p.OnClosing, () => onClosingCallCount++));

            // Act
            await Task.WhenAll(
                popup.Instance.CloseAsync(),
                popup.Instance.CloseAsync(),
                popup.Instance.CloseAsync());

            // Assert
            onClosingCallCount.Should().Be(1);
        }
    }
}
