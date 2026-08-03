using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Mocks;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Components;

/// <summary>
/// Tests for types of <typeparamref name="TComponent"/> which implement <see cref="IDropzone"/> and
/// is a drop target specified by <typeparamref name="TDropTargetInterface"/>.
/// </summary>
public sealed class DropzoneTests<TComponent, TDropTargetInterface>(string cssSelector)
    where TComponent : class, IComponent, IDropzone, IAsyncDisposable, TDropTargetInterface
{
    /// <summary>
    /// Asserts that the event handler for event <see cref="IDragInteraction.DragStart"/> is assigned
    /// after the first render of the component.
    /// </summary>
    public void ShouldAssignEventHandlerForDragStart(Action<IServiceCollection>? configureServices = null,
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var dragInteraction = new DragInteractionMock();

        using var testContext = new BunitContext();
        testContext.Services.AddScoped<IDragInteraction>(_ => dragInteraction)
            .AddScoped(_ => Substitute.For<IDropPolicy<TDropTargetInterface>>())
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        configureServices?.Invoke(testContext.Services);

        // Act
        testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        // Assert
        dragInteraction.IsDragStartEventHandlerAssigned.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the event handler for event <see cref="IDragInteraction.DragStart"/>
    /// is removed when the component is disposed.
    /// </summary>
    public async Task ShouldRemoveEventHandlerForDragStartOnDisposeAsync(Action<IServiceCollection>? configureServices = null,
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var dragInteraction = new DragInteractionMock();

        await using var testContext = new BunitContext();
        testContext.Services.AddScoped<IDragInteraction>(_ => dragInteraction)
            .AddScoped(_ => Substitute.For<IDropPolicy<TDropTargetInterface>>())
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        configureServices?.Invoke(testContext.Services);

        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        var componentInstance = renderedComponent.Instance;

        // Act
        await componentInstance.DisposeAsync();

        // Assert
        dragInteraction.IsDragStartEventHandlerAssigned.Should().BeFalse();
    }

    /// <summary>
    /// Asserts that the component indicates it is a dropzone and registers itself as such on drag start.
    /// </summary>
    public void ShouldIndicateDropzoneAndRegisterItselfAsSuchOnDragStart(BunitContext testContext,
        string modifierCssClass = "indicate-dropzone",
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var draggable = Substitute.For<IDraggable>();
        var dragInteraction = new DragInteractionMock();

        var dropPolicy = Substitute.For<IDropPolicy<TDropTargetInterface>>();

        testContext.Services.AddScoped<IDragInteraction>(_ => dragInteraction)
            .AddScoped(_ => dropPolicy)
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        dropPolicy.Accepts(draggable, renderedComponent.Instance).Returns(true);

        // Act
        dragInteraction.StartDrag(draggable, out var dropzones);

        // Assert
        var canvasItem = renderedComponent.Find(cssSelector);

        canvasItem.GetAttribute("class").Should().Contain(modifierCssClass);

        dropzones.Should().Contain(renderedComponent.Instance);
    }

    /// <summary>
    /// Asserts that the component indicates that a draggable is dragged over it
    /// when event handler <see cref="IDropzone.DragEnterAsync"/> is called by
    /// the presence of <paramref name="modifierCssClass"/> in the class attribute
    /// of the most outer HTML element of the component.
    /// </summary>
    public async Task ShouldIndicateDragEnteredAsync(BunitContext testContext,
        string modifierCssClass = "drag-entered",
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var draggable = Substitute.For<IDraggable>();

        testContext.Services.AddScoped(_ => Substitute.For<IDragInteraction>())
            .AddScoped(_ => Substitute.For<IDropPolicy<TDropTargetInterface>>())
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        // Act
        await renderedComponent.Instance.DragEnterAsync(draggable);

        // Assert
        var canvasItem = renderedComponent.Find(cssSelector);

        canvasItem.GetAttribute("class").Should().Contain(modifierCssClass);
    }

    /// <summary>
    /// Asserts that the component indicates that a draggable is not dragged over it anymore
    /// when event handler <see cref="IDropzone.DragLeaveAsync"/> is called by the absence
    /// of <paramref name="modifierCssClass"/> in the class attribute of the most outer
    /// HTML element of the component.
    /// </summary>
    public async Task ShouldNotIndicateDragEnteredAfterDragLeaveAsync(BunitContext testContext,
        string modifierCssClass = "drag-entered",
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var draggable = Substitute.For<IDraggable>();

        testContext.Services.AddScoped(_ => Substitute.For<IDragInteraction>())
            .AddScoped(_ => Substitute.For<IDropPolicy<TDropTargetInterface>>())
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        // Act
        await renderedComponent.Instance.DragEnterAsync(draggable);
        await renderedComponent.Instance.DragLeaveAsync();

        // Assert
        var canvasItem = renderedComponent.Find(cssSelector);

        canvasItem.GetAttribute("class").Should().NotContain(modifierCssClass);
    }

    /// <summary>
    /// Asserts that the component indicates that a draggable is not dragged over it anymore
    /// when event handler <see cref="IDropzone.DragDroppedAsync"/> is called and that the
    /// drop handler is called with the correct parameters.
    /// </summary>
    public async Task AssertDragDroppedHandlingAsync(BunitContext testContext,
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var draggable = Substitute.For<IDraggable>();
        var dragInteraction = new DragInteractionMock();
        const int DropPositionX = 15;
        const int DropPositionY = 35;

        var dropPolicy = Substitute.For<IDropPolicy<TDropTargetInterface>>();
        var dropHandler = Substitute.For<IDropHandler<TDropTargetInterface>>();

        testContext.Services.AddScoped<IDragInteraction>(_ => dragInteraction)
            .AddScoped(_ => dropPolicy)
            .AddScoped(_ => dropHandler);

        var renderedComponent = testContext.Render<TComponent>(
            b => configureComponentParameters?.Invoke(b));

        dropPolicy.Accepts(draggable, renderedComponent.Instance).Returns(true);

        // Act
        dragInteraction.StartDrag(draggable, out var dropzones);

        var dropzone = dropzones[0];
        await dropzone.DragDroppedAsync(draggable, DropPositionX, DropPositionY);

        // Assert
        var canvasItem = renderedComponent.Find(cssSelector);

        canvasItem.GetAttribute("class").Should().NotContain("indicate-dropzone").And.NotContain("drag-entered");

        dropPolicy.Received().Accepts(draggable, renderedComponent.Instance);

        await dropHandler.Received().DragDroppedAsync(draggable, DropPositionX, DropPositionY, renderedComponent.Instance);
    }

    /// <summary>
    /// Asserts that the component indicates that a draggable is not dragged over it anymore
    /// when event handler <see cref="IDropzone.DragEndAsync"/> is called.
    /// </summary>
    public async Task ShouldNotIndicateDropzoneAndDragEnteredAfterDragEndAsync(BunitContext testContext,
        string indicateDropZoneModifierCssClass = "indicate-dropzone",
        string dragEnteredModifierCssClass = "drag-entered",
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var draggable = Substitute.For<IDraggable>();
        var dragInteraction = new DragInteractionMock();

        var dropPolicy = Substitute.For<IDropPolicy<TDropTargetInterface>>();

        testContext.Services.AddScoped<IDragInteraction>(_ => dragInteraction)
            .AddScoped(_ => dropPolicy)
            .AddScoped(_ => Substitute.For<IDropHandler<TDropTargetInterface>>());

        var renderedComponent = testContext.Render<TComponent>(
            b => configureComponentParameters?.Invoke(b));

        dropPolicy.Accepts(draggable, renderedComponent.Instance).Returns(true);

        // Act
        dragInteraction.StartDrag(draggable, out var dropzones);

        var dragEndTasks = dropzones.Select(dropzone => dropzone.DragEndAsync(draggable, 0, 0));
        await Task.WhenAll(dragEndTasks);

        // Assert
        var canvasItem = renderedComponent.Find(cssSelector);

        canvasItem.GetAttribute("class")
            .Should()
            .NotContain(indicateDropZoneModifierCssClass)
            .And
            .NotContain(dragEnteredModifierCssClass);
    }
}
