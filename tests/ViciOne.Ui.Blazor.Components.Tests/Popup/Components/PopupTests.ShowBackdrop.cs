using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class ShowBackdrop : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public ShowBackdrop()
        {
            _testContext.Services.AddPopup();
            _testContext.JSInterop.SetupForPopup();
        }

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Should_trap_focus_only_with_backdrop(bool showBackdrop)
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.ShowBackdrop, showBackdrop)
                .Add(p => p.Visible, true));

            // Act
            popup.RenderSectionContent(_testContext);

            popup.Render(b => b
                .Add(p => p.ShowBackdrop, showBackdrop)
                .Add(p => p.Visible, true));

            // Assert
            _testContext.JSInterop.Invocations.Count(i => i.Identifier == "attach")
                .Should().Be(showBackdrop ? 1 : 0);
        }

        [Fact]
        public void Should_release_focus_trap_when_backdrop_is_hidden()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.ShowBackdrop, true)
                .Add(p => p.Visible, true));

            popup.RenderSectionContent(_testContext);

            popup.Render(b => b
                .Add(p => p.ShowBackdrop, true)
                .Add(p => p.Visible, true));

            // Act
            popup.Render(b => b
                .Add(p => p.ShowBackdrop, false)
                .Add(p => p.Visible, true));

            // Assert
            _testContext.JSInterop.Invocations.Should().Contain(i => i.Identifier == "dispose");
        }
    }
}
