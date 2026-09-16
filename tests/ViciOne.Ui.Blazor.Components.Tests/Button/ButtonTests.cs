using Bunit;
using Microsoft.AspNetCore.Components;

using ButtonComponent = ViciOne.Ui.Blazor.Components.Button.Button;

namespace ViciOne.Ui.Blazor.Components.Tests.Button;

public sealed partial class ButtonTests
{
    [Fact]
    public void Should_render_disabled_attribute_when_not_enabled()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<ButtonComponent>(b => b
            .Add(p => p.Enabled, false));

        // Assert
        renderedComponent.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_raise_on_click()
    {
        // Arrange
        using var testContext = new BunitContext();
        var clickCount = 0;

        var renderedComponent = testContext.Render<ButtonComponent>(b => b
            .Add(p => p.OnClick, EventCallback.Factory.Create(this, () => clickCount++)));

        // Act
        renderedComponent.Find("button").Click();

        // Assert
        clickCount.Should().Be(1);
    }
}
