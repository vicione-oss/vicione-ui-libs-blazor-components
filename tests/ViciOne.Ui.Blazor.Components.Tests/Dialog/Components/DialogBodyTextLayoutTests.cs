using Bunit;
using ViciOne.Ui.Blazor.Components.Dialog.Components;

namespace ViciOne.Ui.Blazor.Components.Tests.Dialog.Components;

public sealed class DialogBodyTextLayoutTests
{
    [Fact]
    public void Should_render_root_element()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogBodyTextLayout>();

        // Assert
        renderedComponent.Find(".dialog-body-text-layout");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("<p class=\"content\">Hello</p>")]
    public void Should_render_child_content(string? childContent)
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<DialogBodyTextLayout>(b =>
        {
            if (childContent is not null)
                b.AddChildContent(childContent);
        });

        var element = renderedComponent.Find(".dialog-body-text-layout");

        // Assert
        if (childContent is not null)
            element.InnerHtml.Should().Contain("Hello");
        else
            element.InnerHtml.Trim().Should().BeEmpty();
    }
}
