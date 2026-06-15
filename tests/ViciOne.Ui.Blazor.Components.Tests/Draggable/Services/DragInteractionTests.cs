using System.Linq.Expressions;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Extensions;
using ViciOne.Ui.Blazor.Components.Draggable.Models;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Draggable.Services;

public sealed class DragInteractionTests
{
    private readonly string _jsModuleIdentifier =
        $"./_content/{typeof(DragInteraction).Assembly.GetName().Name}/draggable/drag-interaction.js";

    [Fact]
    public async Task Should_import_js_module_on_first_attach()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act
        await dragInteraction.AttachAsync(draggable);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> matcher = i =>
            i.Identifier == "import" && i.Arguments.OfType<string>().First() == _jsModuleIdentifier;

        testContext.JSInterop.Invocations.Should().Contain(matcher);
    }

    [Fact]
    public async Task Should_not_import_js_module_on_subsequent_attach()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable1 = new TestDraggable();
        var draggable2 = new TestDraggable();

        await dragInteraction.AttachAsync(draggable1);
        var importCountAfterFirst = testContext.JSInterop.Invocations.Count(i => i.Identifier == "import");

        // Act
        await dragInteraction.AttachAsync(draggable2);

        // Assert
        testContext.JSInterop.Invocations.Count(i => i.Identifier == "import")
            .Should().Be(importCountAfterFirst);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ModifierKey.Alt)]
    public async Task Should_invoke_js_attach_on_attach(ModifierKey? modifierKey)
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act
        await dragInteraction.AttachAsync(draggable, modifierKey);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> matcher = i => i.Identifier == "attach";
        jsModule.Invocations.Should().Contain(matcher);
    }

    [Fact]
    public async Task Should_pass_null_behaviors_to_js_attach_when_not_provided()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act
        await dragInteraction.AttachAsync(draggable);

        // Assert
        var invocation = jsModule.Invocations.First(i => i.Identifier == "attach");
        ((DragInteractionContext)invocation.Arguments[0]!).PointerCaptureBehaviors.Should().BeNull();
    }

    [Fact]
    public async Task Should_pass_behavior_js_objects_to_js_attach()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var behaviorJsObject = Substitute.For<IJSObjectReference>();
        var behavior = Substitute.For<IPointerCaptureBehavior>();
        behavior.GetJsObjectAsync().Returns(Task.FromResult<IJSObjectReference?>(behaviorJsObject));

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act
        await dragInteraction.AttachAsync(draggable, pointerCaptureBehaviors: [behavior]);

        // Assert
        var invocation = jsModule.Invocations.First(i => i.Identifier == "attach");
        ((DragInteractionContext)invocation.Arguments[0]!).PointerCaptureBehaviors.Should().BeEquivalentTo([behaviorJsObject]);
    }

    [Fact]
    public async Task Should_filter_behaviors_that_return_null_js_object()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var behavior = Substitute.For<IPointerCaptureBehavior>();
        behavior.GetJsObjectAsync().Returns(Task.FromResult<IJSObjectReference?>(null));

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act
        await dragInteraction.AttachAsync(draggable, pointerCaptureBehaviors: [behavior]);

        // Assert
        var invocation = jsModule.Invocations.First(i => i.Identifier == "attach");
        ((DragInteractionContext)invocation.Arguments[0]!).PointerCaptureBehaviors.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_not_reinvoke_js_attach_when_already_attached()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        await dragInteraction.AttachAsync(draggable);
        var attachCountAfterFirst = jsModule.Invocations.Count(i => i.Identifier == "attach");

        // Act — attach same draggable again
        await dragInteraction.AttachAsync(draggable);

        // Assert
        jsModule.Invocations.Count(i => i.Identifier == "attach")
            .Should().Be(attachCountAfterFirst);
    }

    [Fact]
    public async Task Should_not_throw_when_removing_non_attached_draggable()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        // Act & Assert
        var act = async () => await dragInteraction.RemoveAsync(draggable);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_attach_result_on_remove()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        await dragInteraction.AttachAsync(draggable);

        // Act
        await dragInteraction.RemoveAsync(draggable);

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> matcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(matcher);
    }

    [Fact]
    public async Task Should_invoke_dispose_on_js_attach_result_on_dispose_async()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();

        await dragInteraction.AttachAsync(draggable);

        // Act
        await ((IAsyncDisposable)dragInteraction).DisposeAsync();

        // Assert
        Expression<Func<JSRuntimeInvocation, bool>> matcher = i => i.Identifier == "dispose";
        testContext.JSInterop.Invocations.Should().Contain(matcher);
    }

    [Fact]
    public async Task OnDragStart_fires_drag_start_event_and_returns_registered_dropzones()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var dropzone = Substitute.For<IDropzone>();
        var expectedElementRef = new ElementReference("test-dropzone-ref");
        dropzone.GetElementReference().Returns(expectedElementRef);

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) => args.Dropzones.Add(dropzone);

        // Act
        var descriptors = await dragInteraction.DragStartAsync(draggableId);

        // Assert
        descriptors.Should().HaveCount(1);

        var firstDescriptor = descriptors.First();
        firstDescriptor.Id.Should().NotBe(Guid.Empty);
        firstDescriptor.Element.Should().Be(expectedElementRef);
    }

    [Fact]
    public async Task OnDragEnter_calls_drag_enter_on_matching_dropzone()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var dropzone = Substitute.For<IDropzone>();

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) => args.Dropzones.Add(dropzone);

        var descriptors = await dragInteraction.DragStartAsync(draggableId);
        var dropzoneId = descriptors.First().Id;

        // Act
        await dragInteraction.DragEnterAsync(draggableId, dropzoneId);

        // Assert
        await dropzone.Received().DragEnterAsync(draggable);
    }

    [Fact]
    public async Task OnDragLeave_calls_drag_leave_on_matching_dropzone()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var dropzone = Substitute.For<IDropzone>();

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) => args.Dropzones.Add(dropzone);

        var descriptors = await dragInteraction.DragStartAsync(draggableId);
        var dropzoneId = descriptors.First().Id;

        // Act
        await dragInteraction.DragLeaveAsync(dropzoneId);

        // Assert
        await dropzone.Received().DragLeaveAsync();
    }

    [Fact]
    public async Task OnDragEndAsync_calls_drag_end_on_all_dropzones()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var dropzone1 = Substitute.For<IDropzone>();
        var dropzone2 = Substitute.For<IDropzone>();
        const double X = 10;
        const double Y = 20;

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) =>
        {
            args.Dropzones.Add(dropzone1);
            args.Dropzones.Add(dropzone2);
        };

        await dragInteraction.DragStartAsync(draggableId);

        // Act
        await dragInteraction.DragEndAsync(draggableId, X, Y);

        // Assert
        await dropzone1.Received().DragEndAsync(draggable, X, Y);
        await dropzone2.Received().DragEndAsync(draggable, X, Y);
    }

    [Fact]
    public async Task OnDragDroppedAsync_calls_drag_dropped_on_target_dropzone_only()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var targetDropzone = Substitute.For<IDropzone>();
        var otherDropzone = Substitute.For<IDropzone>();
        const double X = 15;
        const double Y = 35;

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) =>
        {
            args.Dropzones.Add(otherDropzone);   // added first → last after reverse
            args.Dropzones.Add(targetDropzone);  // added last → first after reverse
        };

        var descriptors = await dragInteraction.DragStartAsync(draggableId);
        var targetDropzoneId = descriptors.First().Id;

        // Act
        await dragInteraction.DragDroppedAsync(draggableId, targetDropzoneId, X, Y);

        // Assert
        await targetDropzone.Received().DragDroppedAsync(draggable, X, Y);
        await otherDropzone.DidNotReceive().DragDroppedAsync(Arg.Any<IDraggable>(), Arg.Any<double>(), Arg.Any<double>());
    }

    [Fact]
    public async Task Should_not_deadlock_when_remove_is_called_from_drag_dropped_handler()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.MockServicesForDragInteraction().AddDraggable();

        var jsModule = testContext.JSInterop.SetupModule(_jsModuleIdentifier);
        jsModule.SetupModule("attach", _ => true);
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var dragInteraction = (DragInteraction)testContext.Services.GetRequiredService<IDragInteraction>();
        var draggable = new TestDraggable();
        var dropzone = Substitute.For<IDropzone>();
        const double X = 10;
        const double Y = 20;

        dropzone.DragDroppedAsync(draggable, X, Y)
            .Returns(_ => dragInteraction.RemoveAsync(draggable));

        await dragInteraction.AttachAsync(draggable);

        var draggableId = ((DragInteractionContext)jsModule.Invocations.First(i => i.Identifier == "attach").Arguments[0]!).DraggableId;

        dragInteraction.DragStart += (_, args) => args.Dropzones.Add(dropzone);

        var descriptors = await dragInteraction.DragStartAsync(draggableId);
        var dropzoneId = descriptors.First().Id;

        // Act — would deadlock before the fix because RemoveAsync re-enters ExecuteGuardedAsync
        var act = async () => await dragInteraction.DragDroppedAsync(draggableId, dropzoneId, X, Y);
        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    }

#pragma warning disable RCS1060 // Declare each type in separate file
    private sealed class TestDraggable : IDraggable
    {
        public bool Draggable => true;
        public ElementReference GetElementReference() => default;
    }
#pragma warning restore RCS1060
}
