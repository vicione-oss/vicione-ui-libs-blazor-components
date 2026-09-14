using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.Resizeable.Components;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;
using ViciOne.Ui.Blazor.Components.Sidebar.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Sidebar.Extensions;
using SidebarComponent = ViciOne.Ui.Blazor.Components.Sidebar.Sidebar;

namespace ViciOne.Ui.Blazor.Components.Tests.Components;

public sealed class SidebarTests : IAsyncDisposable
{
    public static readonly TheoryData<string> SidebarPlacements =
        [.. TypeSafeEnumFactory<SidebarPlacement>.CreateAll().Select(placement => placement.GetName())];

    private readonly BunitContext _testContext;

    public SidebarTests()
    {
        _testContext = new BunitContext();
        _testContext.Services.AddSidebar();

        _testContext.JSInterop.SetupForSidebar();
    }

    public ValueTask DisposeAsync()
        => _testContext.DisposeAsync();

    [Fact]
    public void Should_render_component()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>();

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

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Placement, placementTyped));

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("class").Should().Contain(modifierCssClass);
    }

    [Fact]
    public void Should_render_with_compact_width()
    {
        // Arrange
        const int CompactWidth = 50;

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
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
        const int FluidMinimumWidth = 100;
        const int FluidMaximumWidth = 200;

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
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
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
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
        const string ContentText = "Lorem ipsum";

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .AddChildContent(ContentText));

        var content = renderedComponent.Find(".content");

        // Assert
        content.TextContent.Should().Contain(ContentText);
    }

    [Fact]
    public void Should_render_resize_handle_in_fluid_mode()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 100)
            .Add(s => s.FluidMaximumWidth, 200));

        // Assert
        renderedComponent.FindAll(".resize-handle").Should().ContainSingle();
    }

    [Fact]
    public void Should_not_render_resize_handle_in_compact_mode()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Compact)
            .Add(s => s.CompactWidth, 50));

        // Assert
        renderedComponent.FindAll(".resize-handle").Should().BeEmpty();
    }

    [Fact]
    public void Should_render_resize_handle_on_the_right_edge_for_left_placement()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Placement, SidebarPlacement.Left)
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 100)
            .Add(s => s.FluidMaximumWidth, 200));

        var resizeHandle = renderedComponent.Find(".resize-handle");

        // Assert
        resizeHandle.GetAttribute("class").Should().Contain("right");
    }

    [Fact]
    public void Should_render_resize_handle_on_the_left_edge_for_right_placement()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Placement, SidebarPlacement.Right)
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 100)
            .Add(s => s.FluidMaximumWidth, 200));

        var resizeHandle = renderedComponent.Find(".resize-handle");

        // Assert
        resizeHandle.GetAttribute("class").Should().Contain("left");
    }

    [Fact]
    public void Should_render_resize_container_spanning_the_fluid_maximum_width()
    {
        // Arrange
        const int FluidMaximumWidth = 300;

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, FluidMaximumWidth));

        var resizeContainer = renderedComponent.Find(".resize-container");

        // Assert
        resizeContainer.GetAttribute("style").Should().Contain($"width: {FluidMaximumWidth}px");
    }

    [Fact]
    public void Should_hand_the_fluid_minimum_width_to_the_resize_interaction()
    {
        // Arrange
        const int FluidMinimumWidth = 200;

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, FluidMinimumWidth)
            .Add(s => s.FluidMaximumWidth, 300));

        var resizeable = (IResizeable)renderedComponent.Instance;

        // Assert
        resizeable.GetMinimumWidth().Should().Be(FluidMinimumWidth);
    }

    [Fact]
    public void Should_be_its_own_resize_container()
    {
        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        var resizeable = (IResizeable)renderedComponent.Instance;

        // Assert
        resizeable.GetResizeContainer().Should().BeSameAs(renderedComponent.Instance);
    }

    [Fact]
    public async Task Should_render_width_reported_by_resize_interaction()
    {
        // Arrange
        const int DraggedWidth = 250;

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        await UpdateWidthAsync(renderedComponent.Instance, DraggedWidth);

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        sidebar.GetAttribute("style").Should().Contain($"--width: {DraggedWidth}px");
    }

    [Fact]
    public async Task Should_raise_fluid_width_changed_for_width_reported_by_resize_interaction()
    {
        // Arrange
        const int DraggedWidth = 250;

        int? changedFluidWidth = null;

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300)
            .Add(s => s.FluidWidthChanged, width => changedFluidWidth = width));

        // Act
        await UpdateWidthAsync(renderedComponent.Instance, DraggedWidth);

        // Assert
        changedFluidWidth.Should().Be(DraggedWidth);
    }

    [Fact]
    public async Task Should_clamp_width_reported_by_resize_interaction_to_fluid_maximum_width()
    {
        // Arrange
        const int FluidMaximumWidth = 300;

        int? changedFluidWidth = null;

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, FluidMaximumWidth)
            .Add(s => s.FluidWidthChanged, width => changedFluidWidth = width));

        // Act
        await UpdateWidthAsync(renderedComponent.Instance, FluidMaximumWidth + 500);

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        changedFluidWidth.Should().Be(FluidMaximumWidth);
        sidebar.GetAttribute("style").Should().Contain($"--width: {FluidMaximumWidth}px");
    }

    [Fact]
    public async Task Should_clamp_width_reported_by_resize_interaction_to_fluid_minimum_width()
    {
        // Arrange
        const int FluidMinimumWidth = 200;

        int? changedFluidWidth = null;

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, FluidMinimumWidth)
            .Add(s => s.FluidMaximumWidth, 300)
            .Add(s => s.FluidWidthChanged, width => changedFluidWidth = width));

        // Act
        await UpdateWidthAsync(renderedComponent.Instance, FluidMinimumWidth - 500);

        var sidebar = renderedComponent.Find(".sidebar");

        // Assert
        changedFluidWidth.Should().Be(FluidMinimumWidth);
        sidebar.GetAttribute("style").Should().Contain($"--width: {FluidMinimumWidth}px");
    }

    [Fact]
    public async Task Should_attach_resize_interaction_in_fluid_mode()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        // Act
        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Assert
        await resizeInteraction.Received(1).AttachAsync(renderedComponent.Instance,
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_not_attach_resize_interaction_in_compact_mode()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        // Act
        _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Compact)
            .Add(s => s.CompactWidth, 50));

        // Assert
        await resizeInteraction.DidNotReceive().AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_attach_resize_interaction_only_once_across_re_renders()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        renderedComponent.Render();
        renderedComponent.Render();
        renderedComponent.Render();

        // Assert
        await resizeInteraction.Received(1).AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_re_attach_resize_interaction_when_fluid_minimum_width_changed()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        renderedComponent.Render(b => b
            .Add(s => s.FluidMinimumWidth, 250));

        // Assert
        await resizeInteraction.Received(1).RemoveAsync(renderedComponent.Instance);
        await resizeInteraction.Received(2).AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_re_attach_resize_interaction_when_placement_changed()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Placement, SidebarPlacement.Left)
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        renderedComponent.Render(b => b
            .Add(s => s.Placement, SidebarPlacement.Right));

        // Assert
        await resizeInteraction.Received(1).RemoveAsync(renderedComponent.Instance);
        await resizeInteraction.Received(2).AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_not_re_attach_resize_interaction_when_fluid_maximum_width_changed()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        renderedComponent.Render(b => b
            .Add(s => s.FluidMaximumWidth, 400));

        // Assert
        await resizeInteraction.Received(1).AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    [Fact]
    public async Task Should_remove_resize_interaction_when_mode_switched_to_compact()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        renderedComponent.Render(b => b
            .Add(s => s.Mode, SidebarMode.Compact));

        // Assert
        await resizeInteraction.Received(1).RemoveAsync(renderedComponent.Instance);
    }

    [Fact]
    public async Task Should_remove_resize_interaction_when_disposed()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Act
        await renderedComponent.Instance.DisposeAsync();

        // Assert
        await resizeInteraction.Received(1).RemoveAsync(renderedComponent.Instance);
    }

    [Fact]
    public async Task Should_not_attach_resize_interaction_when_disposed_before_switching_to_fluid_mode()
    {
        // Arrange
        var resizeInteraction = AddResizeInteractionSubstitute();

        var renderedComponent = _testContext.Render<SidebarComponent>(b => b
            .Add(s => s.Mode, SidebarMode.Compact)
            .Add(s => s.CompactWidth, 50));

        await renderedComponent.Instance.DisposeAsync();

        // Act
        renderedComponent.Render(b => b
            .Add(s => s.Mode, SidebarMode.Fluid)
            .Add(s => s.FluidMinimumWidth, 200)
            .Add(s => s.FluidMaximumWidth, 300));

        // Assert
        await resizeInteraction.DidNotReceive().AttachAsync(Arg.Any<IResizeable>(),
            Arg.Any<IEnumerable<IPointerCaptureBehavior>?>());
    }

    private IResizeInteraction AddResizeInteractionSubstitute()
    {
        var resizeInteraction = Substitute.For<IResizeInteraction>();
        _testContext.Services.AddSingleton(resizeInteraction);

        return resizeInteraction;
    }

    private static Task UpdateWidthAsync(SidebarComponent sidebar, int width)
        => ((IResizeable)sidebar).UpdatePositionAndSizeAsync(new DomRect { Width = width });
}
