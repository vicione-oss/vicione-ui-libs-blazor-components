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
    public void ShouldRenderComponent()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>();

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(SidebarPlacements))]
    public void ShouldRenderPlacementModifierCssClass(string placement)
    {
        // Arrange
        var placementTyped = TypeSafeEnumFactory<SidebarPlacement>.Create(placement);
        var modifierCssClass = placementTyped.ToModifierCssClass();

        using var testContext = new Bunit.TestContext();

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>(
            b => b.Add(s => s.Placement, placementTyped));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("class").Should().Contain(modifierCssClass);
    }

    [Fact]
    public void ShouldRenderWithCompactWidth()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();

        const int CompactWidth = 50;

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Compact)
            .Add(s => s.CompactWidth, CompactWidth));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain($"--width: {CompactWidth}px");
    }

    [Fact]
    public void ShouldRenderWithFluidMinimumWidthByDefault()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();

        const int FluidMinimumWidth = 100;
        const int FluidMaximumWidth = 200;

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, FluidMinimumWidth)
            .Add(s => s.FluidMaximumWidth, FluidMaximumWidth));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain($"--width: {FluidMinimumWidth}px");
    }

    [Fact]
    public void ShouldCorrectFluidMinimumWidthToBelowOrEqualFluidMaximumWidth()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 300)
            .Add(s => s.FluidMaximumWidth, 200));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain("--width: 200px");
    }

    [Fact]
    public void ShouldRenderChildContent()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();

        const string ContentText = "Lorem ipsum";

        // Act
        var renderedComponent = testContext.RenderComponent<SidebarComponent>(
            b => b.AddChildContent(ContentText));

        var content = renderedComponent.Find(".content");

        // Assert
        ContentText.Should().Contain(ContentText);
    }
}
