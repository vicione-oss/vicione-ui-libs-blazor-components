using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Draggable.Components;

public sealed class DragGhostBaseTests
{
    [Fact]
    public void Should_render_content_as_part_of_the_normal_render_cycle()
    {
        // Arrange
        using var testContext = new BunitContext();

        // Act
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span class=\"payload-marker\">x</span>"));

        // Assert — content renders with the host, no gesture required
        cut.Markup.Should().Contain("drag-ghost");
        cut.Markup.Should().Contain("payload-marker");
    }

    [Fact]
    public async Task Should_render_content_after_render_call()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span class=\"payload-marker\">x</span>"));

        // Act
        await cut.Instance.RenderContentForTestAsync();

        // Assert
        cut.Markup.Should().Contain("payload-marker");
    }

    [Fact]
    public async Task Should_reflect_updated_content_on_a_later_render_call()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        // The markup reads a mutable value, so re-evaluating it yields whatever is current — proving the
        // content is evaluated per gesture, not captured at attach.
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span class=\"val\">first</span>"));

        await cut.Instance.RenderContentForTestAsync();
        cut.Markup.Should().Contain("first");

        // Act
        cut.Render(ps => ps
            .Add(c => c.ContentMarkup, "<span class=\"val\">second</span>"));
        await cut.Instance.RenderContentForTestAsync();

        // Assert — the ghost element shows the latest content, not a merge of both
        cut.Markup.Should().Contain("second");
        cut.Markup.Should().NotContain("first");
    }

    [Fact]
    public void Assert_js_module_descriptor()
    {
        // Arrange
        using var testContext = new BunitContext();
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // Act
        var jsModuleDescriptor = cut.Instance.GetJsModule();

        // Assert
        jsModuleDescriptor.ModuleName.Should().EndWith("draggable/components/drag-ghost-base.js");

        jsModuleDescriptor.CreateFunction.Name.Should().Be("createDragGhost");

        var arguments = jsModuleDescriptor.CreateFunction.Args.Should().BeOfType<CreateDragGhostArgs>().Subject;
        arguments.DotNetObject.Should().NotBeNull();
        arguments.ContentElementReference.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_report_no_lifecycle_capabilities_when_not_implemented()
    {
        // Arrange
        using var testContext = new BunitContext();
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // Act
        var jsModuleDescriptor = cut.Instance.GetJsModule();

        // Assert
        var arguments = jsModuleDescriptor.CreateFunction.Args.Should().BeOfType<CreateDragGhostArgs>().Subject;
        arguments.ProcessDragStart.Should().BeFalse();
        arguments.ProcessDragEnd.Should().BeFalse();
    }

    [Fact]
    public void Should_report_lifecycle_capabilities_when_implemented()
    {
        // Arrange
        using var testContext = new BunitContext();
        var cut = testContext.Render<TestLifecycleDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // Act
        var jsModuleDescriptor = cut.Instance.GetJsModule();

        // Assert
        var arguments = jsModuleDescriptor.CreateFunction.Args.Should().BeOfType<CreateDragGhostArgs>().Subject;
        arguments.ProcessDragStart.Should().BeTrue();
        arguments.ProcessDragEnd.Should().BeTrue();
    }

    [Fact]
    public async Task Should_forward_lifecycle_callbacks_only_when_implemented()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var implemented = testContext.Render<TestLifecycleDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));
        var notImplemented = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // Act
        await implemented.Instance.ForwardDragStartAsync();
        await implemented.Instance.ForwardDragEndAsync();
        await notImplemented.Instance.ForwardDragStartAsync();
        await notImplemented.Instance.ForwardDragEndAsync();

        // Assert — the implementing drag ghost recorded both callbacks; the plain one is a no-op
        implemented.Instance.DragStartCount.Should().Be(1);
        implemented.Instance.DragEndCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_complete_a_pending_wait_when_a_lifecycle_listener_renders_content()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var rendering = testContext.Render<RenderingLifecycleDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // The JS ghost keeps a wait pending for the duration of the drag.
        var wait = rendering.Instance.WaitForContentChangeAsync();
        wait.IsCompleted.Should().BeFalse();

        // Act — the listener calls RenderContentAsync, which renders and completes the pending wait
        await rendering.Instance.ForwardDragStartAsync();

        // Assert — the render completed the wait, so the JS ghost swaps the ghost
        (await wait).Should().BeTrue();
    }

    [Fact]
    public async Task Should_leave_the_wait_pending_when_a_lifecycle_listener_does_not_render()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var notRendering = testContext.Render<TestLifecycleDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        var wait = notRendering.Instance.WaitForContentChangeAsync();

        // Act — this listener does not render, so there is nothing to swap
        await notRendering.Instance.ForwardDragStartAsync();

        // Assert — the wait stays pending, so the ghost is not swapped
        wait.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_complete_a_pending_wait_when_content_renders_outside_a_lifecycle_callback()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // The JS ghost keeps a wait pending for the duration of the drag.
        var wait = cut.Instance.WaitForContentChangeAsync();
        wait.IsCompleted.Should().BeFalse();

        // Act — an out-of-band render (e.g. a timer tick) completes the pending wait
        await cut.Instance.RenderContentForTestAsync();

        // Assert — the wait resolves with true so the JS ghost re-fetches and swaps, then waits again
        (await wait).Should().BeTrue();
    }

    [Fact]
    public async Task Should_return_a_pending_change_on_the_next_wait_when_content_rendered_before_it()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        // Act — a render happens before the JS ghost is waiting, so the change is recorded as pending
        await cut.Instance.RenderContentForTestAsync();

        // Assert — the next wait returns immediately, so no change is missed
        (await cut.Instance.WaitForContentChangeAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Should_release_a_pending_wait_with_false_when_disposed()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span>x</span>"));

        var wait = cut.Instance.WaitForContentChangeAsync();

        // Act — disposing completes the pending wait so the JS loop can stop instead of hanging
        cut.Instance.Dispose();

        // Assert
        (await wait).Should().BeFalse();
    }

    [Fact]
    public async Task Should_be_a_no_op_when_render_is_called_after_dispose()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var cut = testContext.Render<TestDragGhost>(ps => ps
            .Add(c => c.ContentMarkup, "<span class=\"payload-marker\">x</span>"));

        cut.Instance.Dispose();

        // Act & Assert — a render after dispose completes without throwing
        var act = async () => await cut.Instance.RenderContentForTestAsync();
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    }

    private class TestDragGhost : DragGhostBase
    {
        [Parameter] public string ContentMarkup { get; set; } = string.Empty;

        public Task RenderContentForTestAsync() => RenderContentAsync();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<DragGhostContent>(0);
            builder.AddComponentParameter(1, nameof(DragGhostContent.ChildContent),
                (RenderFragment)(contentBuilder => contentBuilder
                    .AddMarkupContent(0, ContentMarkup)));
            builder.AddComponentReferenceCapture(2, reference => Content = (DragGhostContent)reference);
            builder.CloseComponent();
        }
    }

    private sealed class TestLifecycleDragGhost
        : TestDragGhost, IDragStartListener, IDragEndListener
    {
        public int DragStartCount { get; private set; }
        public int DragEndCount { get; private set; }

        public Task DragStartAsync()
        {
            DragStartCount++;

            return Task.CompletedTask;
        }

        public Task DragEndAsync()
        {
            DragEndCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class RenderingLifecycleDragGhost : TestDragGhost, IDragStartListener
    {
        public async Task DragStartAsync()
            => await RenderContentAsync();
    }
}
