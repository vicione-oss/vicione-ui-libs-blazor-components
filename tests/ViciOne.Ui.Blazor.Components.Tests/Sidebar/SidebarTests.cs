using Bunit;
using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;
using ViciOne.Ui.Blazor.Components.Sidebar.Extensions;
using Xunit;
using SidebarComponent = ViciOne.Ui.Blazor.Components.Sidebar.Sidebar;

namespace ViciOne.Ui.Blazor.Components.Tests.Components;

public sealed class SidebarTests
{
    public static readonly TheoryData<string> SidebarPlacements =
        [.. TypeSafeEnumFactory<SidebarPlacement>.CreateAll().Select(placement => placement.GetName())];

    [Fact]
    public void Should_render_component()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>();

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(SidebarPlacements))]
    public void Should_render_placement_modifier_css_class(string placement)
    {
        // Arrange
        var placementTyped = TypeSafeEnumFactory<SidebarPlacement>.Create(placement);
        var modifierCssClass = placementTyped.ToModifierCssClass();

        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>(
            b => b.Add(s => s.Placement, placementTyped));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("class").Should().Contain(modifierCssClass);
    }

    [Fact]
    public void Should_render_with_compact_width()
    {
        // Arrange
        using var testContext = new BunitContext();

        const int CompactWidth = 50;

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Compact)
            .Add(s => s.CompactWidth, CompactWidth));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain($"--width: {CompactWidth}px");
    }

    [Fact]
    public void Should_render_with_fluid_minimum_width_by_default()
    {
        // Arrange
        using var testContext = new BunitContext();

        const int FluidMinimumWidth = 100;
        const int FluidMaximumWidth = 200;

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, FluidMinimumWidth)
            .Add(s => s.FluidMaximumWidth, FluidMaximumWidth));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain($"--width: {FluidMinimumWidth}px");
    }

    [Fact]
    public void Should_correct_fluid_minimum_width_to_below_or_equal_fluid_maximum_width()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 300)
            .Add(s => s.FluidMaximumWidth, 200));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain("--width: 200px");
    }

    [Fact]
    public void Should_render_child_content()
    {
        // Arrange
        using var testContext = new BunitContext();

        const string ContentText = "Lorem ipsum";

        // Act
        var renderedComponent = testContext.Render<SidebarComponent>(
            b => b.AddChildContent(ContentText));

        var content = renderedComponent.Find(".content");

        // Assert
        ContentText.Should().Contain(ContentText);
    }
}
