using Bunit;
using AccordionComponent = ViciOne.Ui.Blazor.Components.Accordion.Components.Accordion;

namespace ViciOne.Ui.Blazor.Components.Tests.Accordion.Components;

public sealed class AccordionTests
{
    [Fact]
    public void Should_render_root_element_with_accordion_class()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionComponent>(b => b
            .Add(p => p.ChildContent, _ => { }));

        // Assert
        renderedComponent.Find(".accordion");
    }

    [Fact]
    public void Should_render_child_content()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<AccordionComponent>(b => b
            .Add(p => p.ChildContent, "<div class=\"test-child\">Hello</div>"));

        // Assert
        var child = renderedComponent.Find(".test-child");
        child.TextContent.Should().Be("Hello");
    }
}
