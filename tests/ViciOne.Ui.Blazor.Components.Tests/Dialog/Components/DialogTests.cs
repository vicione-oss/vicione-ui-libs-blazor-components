using AngleSharp.Dom;
using Bunit;
using ViciOne.Ui.Blazor.Components.Dialog.Components;
using ViciOne.Ui.Blazor.Components.Dialog.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Dialog.Extensions;
using DialogComponent = ViciOne.Ui.Blazor.Components.Dialog.Components.Dialog;

namespace ViciOne.Ui.Blazor.Components.Tests.Dialog.Components;

public sealed class DialogTests
{
    [Fact]
    public async Task Should_render_header_text()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "My Dialog Title")
            .Add(p => p.Visible, true));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var headerText = sectionContent.Find(".header .text");
        headerText.TextContent.Trim().Should().Be("My Dialog Title");
    }

    [Fact]
    public async Task Should_render_css_class()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.CssClass, "custom-dialog")
            .Add(p => p.Visible, true));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        sectionContent.Find(".modal-dialog.custom-dialog");
    }

    [Fact]
    public async Task Should_render_width()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Width, "500px")
            .Add(p => p.Visible, true));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var modalDialog = sectionContent.Find(".modal-dialog");
        modalDialog.GetAttribute("style").Should().Contain("width: 500px");
    }

    [Fact]
    public async Task Should_render_height()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Height, "300px")
            .Add(p => p.Visible, true));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var modalDialog = sectionContent.Find(".modal-dialog");
        modalDialog.GetAttribute("style").Should().Contain("height: 300px");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Assert_visible(bool value)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        IElement? layoutElement = null;

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, value));

        if (value)
        {
            var sectionContent = dialog.RenderSectionContent(testContext);
            layoutElement = sectionContent.Find(".popup-dialog-layout");
        }

        // Assert
        if (value)
            Assert.NotNull(layoutElement);
        else
            Assert.Null(layoutElement);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Assert_header_with_slot_css_class(bool withSlot)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b =>
        {
            b.Add(p => p.HeaderText, "Test");
            b.Add(p => p.Visible, true);

            if (withSlot)
                b.Add(p => p.HeaderSlot, "<span>Slot</span>");
        });

        var sectionContent = dialog.RenderSectionContent(testContext);
        var header = sectionContent.Find(".header");

        // Assert
        if (withSlot)
            header.GetAttribute("class").Should().Contain("with-slot");
        else
            header.GetAttribute("class").Should().NotContain("with-slot");
    }

    [Fact]
    public async Task Should_render_header_slot_content()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.HeaderSlot, "<span class=\"custom-slot\">My Slot</span>"));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var slot = sectionContent.Find(".header .slot .custom-slot");
        slot.TextContent.Should().Be("My Slot");
    }

    [Fact]
    public async Task Should_render_body()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.Body, "<div class=\"body-content\">Body Text</div>"));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var body = sectionContent.Find(".body .body-content");
        body.TextContent.Should().Be("Body Text");
    }

    [Fact]
    public async Task Should_render_footer()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.Footer, "<div class=\"footer-content\">Footer Text</div>"));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        var footer = sectionContent.Find(".footer .footer-content");
        footer.TextContent.Should().Be("Footer Text");
    }

    [Fact]
    public async Task Should_render_footer_button()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test dialog")
            .Add(p => p.Visible, true)
            .Add<DialogFooterButton>(p => p.Footer, buttonParameters => buttonParameters
                .Add(button => button.Text, "Ok")));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        sectionContent.FindFooterButton(button => button.Text == "Ok");
    }

    [Fact]
    public async Task Should_render_close_action_button()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true));

        var sectionContent = dialog.RenderSectionContent(testContext);

        // Assert
        sectionContent.Find(".header .action-buttons");
    }

    [Fact]
    public async Task Should_invoke_on_showing_when_becoming_visible()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        var onShowingInvoked = false;

        // Act
        testContext.Render<DialogComponent>(b => b
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
        await using var testContext = new BunitContext();
        testContext.Services.AddDialog();

        var onClosingInvoked = false;

        // Act
        var dialog = testContext.Render<DialogComponent>(b => b
            .Add(p => p.HeaderText, "Test")
            .Add(p => p.Visible, true)
            .Add(p => p.OnClosing, () => onClosingInvoked = true));

        await dialog.InvokeAsync(dialog.Instance.CloseAsync);

        // Assert
        onClosingInvoked.Should().BeTrue();
    }
}
