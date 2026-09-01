using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
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
            var popup = _testContext.Render<PopupComponent>(b => b
                .Add(p => p.Visible, true));

            await popup.Instance.DisposeAsync();

            // Act
            var closeAction = async () => popup.Instance.CloseAsync();

            // Assert
            await closeAction.Should().NotThrowAsync();
        }
    }
}
