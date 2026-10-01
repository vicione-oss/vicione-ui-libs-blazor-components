using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Components;

namespace Shared.Tests.Pages.Draggable.Components;

public class TicketDispenserTests
{
    private readonly DraggableTests<TicketDispenser> _draggableTests = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Assert_drag_interaction_attach_after_first_render(bool draggable)
        => _draggableTests.AssertDragInteractionAttachAfterFirstRender(draggable, modifierKey: null,
            configureComponentParameters: b => b
                .Add(p => p.Draggable, draggable));

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Assert_drag_interaction_remove_on_dispose(bool draggable)
        => await _draggableTests.AssertDragInteractionRemoveOnDisposeAsync(draggable,
            configureComponentParameters: b => b
                .Add(p => p.Draggable, draggable));

    [Fact]
    public async Task Every_drag_start_hands_out_the_next_ticket()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.Services.AddScoped(_ => Substitute.For<IDragInteraction>());

        var rendered = testContext.Render<TicketDispenser>(b => b
            .Add(p => p.HandOutDuration, TimeSpan.Zero));

        // Act
        await rendered.Instance.PrepareDragStartAsync();
        await rendered.Instance.PrepareDragStartAsync();

        // Assert
        rendered.Instance.Ticket.Should().Be(2);
        rendered.Find(".content").TextContent.Trim().Should().Be("Ticket 2");
    }
}
