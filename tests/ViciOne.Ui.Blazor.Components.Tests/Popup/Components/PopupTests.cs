using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;
using Xunit;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Popup.Components;

public sealed partial class PopupTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public PopupTests()
        => _testContext.Services.AddPopup();

    public ValueTask DisposeAsync()
        => _testContext.DisposeAsync();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Assert_visible(bool value)
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
    public async Task Show_async_should_invoke_on_showing()
    {
        // Arrange
        var onShowingInvoked = false;

        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.OnShowing, () => onShowingInvoked = true));

        // Act
        await popup.Instance.ShowAsync();

        // Assert
        onShowingInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task Show_async_should_invoke_visible_changed_with_true()
    {
        // Arrange
        bool? visibleChangedValue = null;

        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.VisibleChanged, value => visibleChangedValue = value));

        // Act
        await popup.Instance.ShowAsync();

        // Assert
        visibleChangedValue.Should().BeTrue();
    }

    [Fact]
    public async Task Show_async_should_not_invoke_on_showing_when_already_visible()
    {
        // Arrange
        var onShowingCallCount = 0;

        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.OnShowing, () => onShowingCallCount++));

        await popup.Instance.ShowAsync();

        // Act
        await popup.Instance.ShowAsync();

        // Assert
        onShowingCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Show_async_should_not_throw_when_disposed()
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
    public async Task Close_async_should_make_popup_closed()
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
    public async Task Close_async_should_invoke_on_closing()
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
    public async Task Close_async_should_invoke_visible_changed_with_false()
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
    public async Task Close_async_should_not_invoke_on_closing_when_already_hidden()
    {
        // Arrange
        var onClosingCallCount = 0;

        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.OnClosing, () => onClosingCallCount++));

        // Act
        await popup.Instance.CloseAsync();

        // Assert
        onClosingCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Concurrent_show_async_calls_should_invoke_on_showing_once()
    {
        // Arrange
        var onShowingCallCount = 0;

        var popup = _testContext.Render<PopupComponent>(b => b
            .Add(p => p.OnShowing, () => onShowingCallCount++));

        // Act
        await Task.WhenAll(
            popup.Instance.ShowAsync(),
            popup.Instance.ShowAsync(),
            popup.Instance.ShowAsync());

        // Assert
        onShowingCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Concurrent_close_async_calls_should_invoke_on_closing_once()
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
