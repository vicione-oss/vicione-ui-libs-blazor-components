using ViciOne.Ui.Blazor.Components.TestingHelpers.Toolbar.Components;
using ViciOne.Ui.Blazor.Components.Toolbar.Components;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Toolbar.Components;

public sealed class ToolbarContentTests
{
    private readonly ToolbarItemTests<ToolbarContent> _tests = new();

    [Fact]
    public void Should_render_css_class()
        => _tests.ShouldRenderCssClass("test-toolbar-content");
}
