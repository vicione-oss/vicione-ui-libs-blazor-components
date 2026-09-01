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
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.CssClass, "test-popup"));

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
    }
}
