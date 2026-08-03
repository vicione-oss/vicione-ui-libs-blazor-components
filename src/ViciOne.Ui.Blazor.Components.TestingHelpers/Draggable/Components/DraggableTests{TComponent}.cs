using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Components;

/// <summary>
/// Tests for types of <typeparamref name="TComponent"/> which implement <see cref="IDraggable"/>
/// </summary>
public class DraggableTests<TComponent>()
    where TComponent : IComponent, IDraggable, IAsyncDisposable
{
    /// <summary>
    /// Asserts that the drag interaction is correctly attached after the first render of the component.
    /// </summary>
    public void AssertDragInteractionAttachAfterFirstRender(bool draggable, ModifierKey? modifierKey,
        Action<IServiceCollection>? configureServices = null,
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var dragInteraction = Substitute.For<IDragInteraction>();

        using var testContext = new BunitContext();
        testContext.Services.AddScoped(_ => dragInteraction);

        configureServices?.Invoke(testContext.Services);

        // Act
        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        // Assert
        if (draggable)
        {
            dragInteraction.Received().AttachAsync(renderedComponent.Instance, modifierKey,
                Arg.Any<IEnumerable<IPointerCaptureBehavior>?>(), Arg.Any<IDragGhost?>());
        }
        else
        {
            dragInteraction.DidNotReceiveWithAnyArgs().AttachAsync(Arg.Any<IDraggable>(), Arg.Any<ModifierKey>());
        }
    }

    /// <summary>
    /// Asserts that the drag interaction is correctly removed when the component is disposed.
    /// </summary>
    public async Task AssertDragInteractionRemoveOnDisposeAsync(bool draggable,
        Action<IServiceCollection>? configureServices = null,
        Action<ComponentParameterCollectionBuilder<TComponent>>? configureComponentParameters = null)
    {
        // Arrange
        var dragInteraction = Substitute.For<IDragInteraction>();

        await using var testContext = new BunitContext();
        testContext.Services.AddScoped(_ => dragInteraction);

        configureServices?.Invoke(testContext.Services);

        var renderedComponent = testContext.Render<TComponent>(b => configureComponentParameters?.Invoke(b));

        var componentInstance = renderedComponent.Instance;

        // Act
        await componentInstance.DisposeAsync();

        // Assert
        if (draggable)
            await dragInteraction.Received().RemoveAsync(componentInstance);
        else
            await dragInteraction.DidNotReceive().RemoveAsync(Arg.Any<IDraggable>());
    }
}
