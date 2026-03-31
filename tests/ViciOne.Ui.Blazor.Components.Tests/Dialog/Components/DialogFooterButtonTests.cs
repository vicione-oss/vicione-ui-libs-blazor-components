using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.Dialog.Components;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Dialog.Components;

public sealed class DialogFooterButtonTests
{
    [Fact]
    public void Should_render_root_element()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
            b.Add(p => p.Text, "OK"));

        // Assert
        renderedComponent.Find(".dialog-footer-button-container");
    }

    [Fact]
    public void Should_render_inner_button_with_default_css_classes()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
            b.Add(p => p.Text, "OK"));

        var button = renderedComponent.Find("button");

        // Assert
        button.GetAttribute("class").Should().Contain("button");
        button.GetAttribute("class").Should().Contain("button--small");
        button.GetAttribute("class").Should().Contain("dialog-footer-button");
    }

    [Fact]
    public void Should_render_id()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
        {
            b.Add(p => p.Text, "OK");
            b.Add(p => p.Id, "my-button");
        });

        var button = renderedComponent.Find("button");

        // Assert
        button.GetAttribute("id").Should().Be("my-button");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("custom-class")]
    [InlineData("first-class second-class")]
    public void Should_render_css_class(string? cssClass)
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
        {
            b.Add(p => p.Text, "OK");

            if (cssClass is not null)
                b.Add(p => p.CssClass, cssClass);
        });

        var button = renderedComponent.Find("button");
        var classes = button.GetAttribute("class")!;

        // Assert
        classes.Should().Contain("dialog-footer-button");

        if (cssClass is not null)
        {
            foreach (var expected in cssClass.Split(" "))
                classes.Should().Contain(expected);
        }
    }

    [Fact]
    public void Should_render_text()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
            b.Add(p => p.Text, "Submit"));

        var textElement = renderedComponent.Find("button .text");

        // Assert
        textElement.TextContent.Should().Be("Submit");
    }

    [Fact]
    public void Should_render_title()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
        {
            b.Add(p => p.Text, "OK");
            b.Add(p => p.Title, "Click to confirm");
        });

        var button = renderedComponent.Find("button");

        // Assert
        button.GetAttribute("title").Should().Be("Click to confirm");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Assert_enabled(bool enabled)
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
        {
            b.Add(p => p.Text, "OK");
            b.Add(p => p.Enabled, enabled);
        });

        var button = renderedComponent.Find("button");

        // Assert
        if (enabled)
            button.GetAttribute("disabled").Should().BeNull();
        else
            button.GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact]
    public void Should_default_enabled_to_true()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
            b.Add(p => p.Text, "OK"));

        var button = renderedComponent.Find("button");

        // Assert
        button.GetAttribute("disabled").Should().BeNull();
    }

    [Fact]
    public void Should_invoke_on_click()
    {
        // Arrange
        using var testContext = new BunitContext();
        var clicked = false;

        var renderedComponent = testContext.Render<DialogFooterButton>(b =>
        {
            b.Add(p => p.Text, "OK");
            b.Add(p => p.OnClick, () => clicked = true);
        });

        var button = renderedComponent.Find("button");

        // Act
        button.Click();

        // Assert
        clicked.Should().BeTrue();
    }
}
