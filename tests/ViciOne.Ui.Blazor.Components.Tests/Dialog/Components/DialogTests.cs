using Bunit;
using ViciOne.Ui.Blazor.Components.Dialog.Components;
using ViciOne.Ui.Blazor.Components.Dialog.Extensions;
using ViciOne.Ui.Blazor.Components.Popup.Components;
using DialogComponent = ViciOne.Ui.Blazor.Components.Dialog.Components.Dialog;
using PopupComponent = ViciOne.Ui.Blazor.Components.Popup.Components.Popup;

namespace ViciOne.Ui.Blazor.Components.Tests.Dialog.Components;

public sealed class DialogTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();
    private readonly IRenderedComponent<PopupRoot> _popupRoot;

    public DialogTests()
    {
        _testContext.Services.AddDialog();

        _popupRoot = _testContext.Render<PopupRoot>();
    }

    public ValueTask DisposeAsync()
    {
        _popupRoot.Dispose();
        return _testContext.DisposeAsync();
    }

    [Fact]
    public void Should_render_header_text()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "My Dialog Title")
            .Add(p => p.Visible, true));

        // Assert
        var headerText = _popupRoot.Find(".header .text");
        headerText.TextContent.Trim().Should().Be("My Dialog Title");
    }

    [Fact]
    public void Should_render_css_class()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.CssClass, "custom-dialog")
            .Add(p => p.Visible, true));

        // Assert
        _popupRoot.Find(".modal-dialog.custom-dialog");
    }

    [Fact]
    public void Should_render_width()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Width, "500px")
            .Add(p => p.Visible, true));

        // Assert
        var modalDialog = _popupRoot.Find(".modal-dialog");
        modalDialog.GetAttribute("style").Should().Contain("width: 500px");
    }

    [Fact]
    public void Should_render_height()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Height, "300px")
            .Add(p => p.Visible, true));

        // Assert
        var modalDialog = _popupRoot.Find(".modal-dialog");
        modalDialog.GetAttribute("style").Should().Contain("height: 300px");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Assert_visible(bool value)
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, value));

        var layoutElements = _popupRoot.FindAll(".popup-dialog-layout");

        // Assert
        if (value)
            layoutElements.Should().ContainSingle();
        else
            layoutElements.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Assert_header_with_slot_css_class(bool withSlot)
    {
        // Act
        _testContext.Render<DialogComponent>(b =>
        {
            b.Add(p => p.HeaderText, "Test");
            b.Add(p => p.Visible, true);

            if (withSlot)
                b.Add(p => p.HeaderSlot, "<span>Slot</span>");
        });

        var header = _popupRoot.Find(".header");

        // Assert
        if (withSlot)
            header.GetAttribute("class").Should().Contain("with-slot");
        else
            header.GetAttribute("class").Should().NotContain("with-slot");
    }

    [Fact]
    public void Should_render_header_slot_content()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.HeaderSlot, "<span class=\"custom-slot\">My Slot</span>"));

        // Assert
        var slot = _popupRoot.Find(".header .slot .custom-slot");
        slot.TextContent.Should().Be("My Slot");
    }

    [Fact]
    public void Should_render_body()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.Body, "<div class=\"body-content\">Body Text</div>"));

        // Assert
        var body = _popupRoot.Find(".body .body-content");
        body.TextContent.Should().Be("Body Text");
    }

    [Fact]
    public void Should_render_footer()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.Footer, "<div class=\"footer-content\">Footer Text</div>"));

        // Assert
        var footer = _popupRoot.Find(".footer .footer-content");
        footer.TextContent.Should().Be("Footer Text");
    }

    [Fact]
    public void Should_render_footer_button()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test dialog")
            .Add(p => p.Visible, true)
            .Add<DialogFooterButton>(p => p.Footer, b => b
                .Add(p => p.Text, "Ok")));

        // Assert
        var button = _popupRoot.FindComponent<DialogFooterButton>().FindComponent<Blazor.Components.Button.Button>();
        button.Instance.Text.Should().BeEquivalentTo("Ok");
    }

    [Fact]
    public void Should_render_close_action_button()
    {
        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true));

        // Assert
        _popupRoot.Find(".header .action-buttons");
    }

    [Fact]
    public void Should_invoke_on_showing_when_becoming_visible()
    {
        // Arrange
        var onShowingInvoked = false;

        // Act
        _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.OnShowing, () => onShowingInvoked = true));

        // Assert
        onShowingInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task Should_invoke_on_closing_when_closing()
    {
        // Arrange
        var onClosingInvoked = false;

        // Act
        var dialog = _testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.OnClosing, () => onClosingInvoked = true));

        await dialog.InvokeAsync(dialog.Instance.CloseAsync);

        // Assert
        onClosingInvoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Should_pass_close_on_escape_to_popup(bool closeOnEscape)
    {
        // Arrange
        // The popup listens for Escape through JS, which bUnit does not run.
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var dialog = _testContext.Render<DialogComponent>(b => b
            .Add(p => p.Visible, true)
            .Add(p => p.CloseOnEscape, closeOnEscape));

        // Assert
        dialog.FindComponent<PopupComponent>().Instance.CloseOnEscape.Should().Be(closeOnEscape);
    }
}
