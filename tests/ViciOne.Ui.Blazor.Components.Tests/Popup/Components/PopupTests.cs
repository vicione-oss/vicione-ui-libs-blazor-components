using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public PopupTests()
    {
        _testContext.Services.AddPopup();
        _testContext.JSInterop.SetupForPopup();
    }

    public ValueTask DisposeAsync()
        => _testContext.DisposeAsync();

    [Fact]
    public async Task Dispose_async_should_cancel_pending_show_async()
    {
        // Arrange
        var popup = _testContext.Render<PopupComponent>();

        var showTask = popup.Instance.ShowAsync();
        var showAction = async () => await showTask;

        // Act
        await popup.Instance.DisposeAsync();

        // Assert
        await showAction.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Dispose_async_should_cancel_pending_close_async()
    {
        // Arrange
        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.Visible, true));

        var closeTask = popup.Instance.CloseAsync();
        var closeAction = async () => await closeTask;

        // Act
        await popup.Instance.DisposeAsync();

        // Assert
        await closeAction.Should().NotThrowAsync();
    }
}
