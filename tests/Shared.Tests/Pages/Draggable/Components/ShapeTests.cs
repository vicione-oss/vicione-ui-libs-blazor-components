using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Components;

namespace Shared.Tests.Pages.Draggable.Components;

public class ShapeTests
{
    private readonly DraggableTests<Shape> _draggableTests = new();

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, ModifierKey.Alt)]
    [InlineData(false, null)]
    public void Assert_drag_interaction_attach_after_first_render(bool draggable, ModifierKey? modifierKey)
        => _draggableTests.AssertDragInteractionAttachAfterFirstRender(draggable, modifierKey,
            configureServices: services =>
                services.AddScoped(_ => Substitute.For<ISnapToGridPointerCaptureBehavior>()),
            configureComponentParameters: p => p
                .Add(c => c.Label, "test")
                .Add(c => c.Draggable, draggable)
                .Add(c => c.ModifierKey, modifierKey));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Assert_drag_interaction_remove_on_dispose(bool draggable)
        => await _draggableTests.AssertDragInteractionRemoveOnDisposeAsync(draggable,
            configureServices: services =>
                services.AddScoped(_ => Substitute.For<ISnapToGridPointerCaptureBehavior>()),
            configureComponentParameters: p => p
                .Add(c => c.Label, "test")
                .Add(c => c.Draggable, draggable));
}
