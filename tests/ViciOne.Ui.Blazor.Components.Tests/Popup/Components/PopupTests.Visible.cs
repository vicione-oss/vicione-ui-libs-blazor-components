using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests
{
    public sealed class Visible : IAsyncDisposable
    {
        private readonly BunitContext _testContext = new();

        public Visible()
            => _testContext.Services.AddPopup();

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Should_control_popup_rendering(bool value)
        {
            // Arrange, Act
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CssClass, "test-popup")
                .Add(p => p.Visible, value));

            // Assert
            if (value)
            {
                var assert = () =>
                {
                    var sectionContent = popup.RenderSectionContent(_testContext);

                    sectionContent.Find(".test-popup");
                };

                assert.Should().NotThrow();
            }
            else
            {
                var assert = () => popup.Find(".test-popup");

                assert.Should().Throw<ElementNotFoundException>();
            }
        }

        [Fact]
        public void Should_focus_modal_dialog()
        {
            // Arrange
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.Visible, true));

            // Act
            popup.RenderSectionContent(_testContext);

            popup.Render(b => b
                .Add(p => p.Visible, true));

            // Assert
            var assert = () => _testContext.JSInterop.VerifyFocusAsyncInvoke();

            assert.Should().NotThrow();
        }
    }
}
