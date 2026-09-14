using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Components;

public sealed class HorizontalScrollContainerTests : IDisposable
{
    private readonly IResizeObserver _resizeObserver;
    private readonly BunitContext _testContext;

    private readonly List<ElementReference> _elementReferences = [];

    public HorizontalScrollContainerTests()
    {
        _resizeObserver = Substitute.For<IResizeObserver>();

        _testContext = new BunitContext();
        _testContext.Services.AddScoped(_ => _resizeObserver);

        _resizeObserver.ObserveAsync(Arg.Do<ElementReference>(_elementReferences.Add));
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void On_resize_sends_scroll_callback()
    {
        // Arrange
        const int MaxScrollStep = 10;
        var receivedScrollEvent = false;
        var scrollStep = 0;

        var renderedComponent = _testContext.Render<HorizontalScrollContainer>(b => b
            .Add(p => p.LastShownPixel, 0)
            .Add(p => p.MaxScrollStepCount, MaxScrollStep)
            .Add(p => p.OnScroll, input => { receivedScrollEvent = true; scrollStep = input; }));

        // Act
        SetContainerSizes(10, 100);

        // simulate scroll appearing and taking some space
        SetContainerSizes(10, 90);
        renderedComponent.Render();

        // Assert
        receivedScrollEvent.Should().BeTrue();
        scrollStep.Should().Be(MaxScrollStep - 1);
    }

    [Theory]
    [InlineData(0, 100, true)]
    [InlineData(1, 100, true)]
    [InlineData(99, 100, true)]
    [InlineData(100, 100, false)]
    [InlineData(101, 100, false)]
    public void Has_scroll_depending_on_content_size(int availableSpace, int contentSize, bool expectScrolling)
    {
        // Arrange
        var renderedComponent = _testContext.Render<HorizontalScrollContainer>(b => b
            .Add(p => p.LastShownPixel, 0)
            .Add(p => p.MaxScrollStepCount, 10)
            .Add(p => p.OnScroll, () => { }));

        // Act
        SetContainerSizes(availableSpace, contentSize);

        renderedComponent.Render(b => b
            .Add(p => p.LastShownPixel, contentSize));

        // Assert
        var hasScroll = renderedComponent.FindAll(".monochrome-icon-expander-light-left").Any();

        hasScroll.Should().Be(expectScrolling);
    }

    [Fact]
    public void On_scroll_click_sends_scroll_callback()
    {
        // Arrange
        int? receivedScrollStep = null;
        var renderedComponent = _testContext.Render<HorizontalScrollContainer>(b => b
            .Add(p => p.LastShownPixel, 0)
            .Add(p => p.MaxScrollStepCount, 10)
            .Add(p => p.OnScroll, input => receivedScrollStep = input));

        SetContainerSizes(10, 100);

        renderedComponent.Render(parameters => parameters
            .Add(p => p.LastShownPixel, 100));

        // simulate scroll appearing and taking some space
        SetContainerSizes(10, 90);
        renderedComponent.Render();

        // Act & Assert
        ScrollLeft(renderedComponent);
        receivedScrollStep.Should().Be(8);

        ScrollLeft(renderedComponent);
        receivedScrollStep.Should().Be(7);

        ScrollRight(renderedComponent);
        receivedScrollStep.Should().Be(8);
    }

    [Fact]
    public void On_scroll_handles_active_state_of_scroll_buttons()
    {
        // Arrange
        var renderedComponent = _testContext.Render<HorizontalScrollContainer>(b => b
            .Add(p => p.LastShownPixel, 0)
            .Add(p => p.MaxScrollStepCount, 3)
            .Add(p => p.OnScroll, _ => { }));

        SetContainerSizes(10, 100);

        // Act & Assert
        renderedComponent.Render(b => b
            .Add(p => p.LastShownPixel, 100));

        // simulate scroll appearing and taking some space
        SetContainerSizes(10, 99);
        renderedComponent.Render();

        renderedComponent.Find(".monochrome-icon-expander-light-left.active").Should().NotBeNull();
        renderedComponent.Find(".monochrome-icon-expander-light-right:not(.active)").Should().NotBeNull();

        ScrollLeft(renderedComponent);

        renderedComponent.Find(".monochrome-icon-expander-light-left.active").Should().NotBeNull();
        renderedComponent.Find(".monochrome-icon-expander-light-right.active").Should().NotBeNull();

        ScrollLeft(renderedComponent);
        renderedComponent.Find(".monochrome-icon-expander-light-left:not(.active)").Should().NotBeNull();
        renderedComponent.Find(".monochrome-icon-expander-light-right.active").Should().NotBeNull();
    }

    private void SetContainerSizes(int outerContainerWidth, int innerContainerWidth)
    {
        var outerContainerElementReference = _elementReferences[0];
        var innerContainerElementReference = _elementReferences[1];

        _resizeObserver.ElementSizeChangedAsync += Raise.Event<Func<ElementSizeChangedEventArgs, Task>>(
            new ElementSizeChangedEventArgs
            {
                ElementReference = outerContainerElementReference,
                DomRect = new() { Width = outerContainerWidth, Height = 50 }
            }
        );

        _resizeObserver.ElementSizeChangedAsync += Raise.Event<Func<ElementSizeChangedEventArgs, Task>>(
            new ElementSizeChangedEventArgs
            {
                ElementReference = innerContainerElementReference,
                DomRect = new DomRect { Width = innerContainerWidth, Height = 50 }
            }
        );
    }

    private static void ScrollLeft(IRenderedComponent<HorizontalScrollContainer> renderedComponent, int stepSizePixels = 10)
    {
        renderedComponent.Find(".monochrome-icon-expander-light-left.active").Click();

        renderedComponent.Render(b => b
            .Add(p => p.LastShownPixel, renderedComponent.Instance.LastShownPixel - stepSizePixels));
    }

    private static void ScrollRight(IRenderedComponent<HorizontalScrollContainer> renderedComponent, int stepSizePixels = 10)
    {
        renderedComponent.Find(".monochrome-icon-expander-light-right.active").Click();

        renderedComponent.Render(b => b
            .Add(p => p.LastShownPixel, renderedComponent.Instance.LastShownPixel + stepSizePixels));
    }
}
