using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Toolbar.Components;
using ViciOne.Ui.Blazor.Components.Toolbar.Components;

namespace ViciOne.Ui.Blazor.Components.Tests.Toolbar.Components;

public sealed class ToolbarButtonTests : IDisposable
{
    private readonly ToolbarItemTests<ToolbarButton> _tests = new();
    private readonly BunitContext _testContext;

    public ToolbarButtonTests()
    {
        var resizeObserver = Substitute.For<IResizeObserver>();

        _testContext = new BunitContext();
        _testContext.Services.AddScoped(_ => resizeObserver);
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render_css_class()
        => _tests.ShouldRenderCssClass("test-toolbar-button");

    [Theory]
    [InlineData("tooltip text", "button text", "tooltip text")]
    [InlineData(null, "button text", "button text")]
    [InlineData("", "button text", "button text")]
    [InlineData("   ", "button text", "button text")]
    [InlineData(null, null, null)]
    public void Assert_title_attribute(string? tooltip, string? text, string? expectedTitle)
    {
        // Arrange
        var toolbarItemParent = Substitute.For<IToolbarItemParent>();

        // Act
        var renderedComponent = _testContext.Render<ToolbarButton>(b =>
        {
            b.AddCascadingValue(toolbarItemParent);

            if (tooltip is not null)
                b.Add(p => p.Tooltip, tooltip);

            if (text is not null)
                b.Add(p => p.Text, text);
        });

        var element = renderedComponent.Find(".toolbar-button");

        // Assert
        element.GetAttribute("title").Should().Be(expectedTitle);
    }
}
