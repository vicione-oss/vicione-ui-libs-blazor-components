using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace Shared.Tests.Pages.Draggable.Services;

public class SampleDropPolicyTests
{
    [Fact]
    public async Task A_ticket_is_accepted_only_by_the_dropzone_serving_it()
    {
        // Arrange
        await using var testContext = CreateContext();
        var dispenser = RenderDispenser(testContext);
        var odd = RenderDropzone(testContext, TicketParity.Odd);
        var even = RenderDropzone(testContext, TicketParity.Even);
        var sut = new SampleDropPolicy();

        await dispenser.PrepareDragStartAsync();

        // Act
        var oddAccepts = sut.Accepts(dispenser, odd);
        var evenAccepts = sut.Accepts(dispenser, even);

        // Assert
        oddAccepts.Should().BeTrue();
        evenAccepts.Should().BeFalse();
    }

    [Fact]
    public async Task The_next_drag_is_decided_by_the_ticket_handed_out_for_it_not_the_previous_one()
    {
        // Arrange
        await using var testContext = CreateContext();
        var dispenser = RenderDispenser(testContext);
        var odd = RenderDropzone(testContext, TicketParity.Odd);
        var even = RenderDropzone(testContext, TicketParity.Even);
        var sut = new SampleDropPolicy();

        await dispenser.PrepareDragStartAsync();
        await dispenser.PrepareDragStartAsync();

        // Act
        var oddAccepts = sut.Accepts(dispenser, odd);
        var evenAccepts = sut.Accepts(dispenser, even);

        // Assert
        oddAccepts.Should().BeFalse();
        evenAccepts.Should().BeTrue();
    }

    [Fact]
    public async Task Any_other_draggable_is_accepted_by_every_dropzone()
    {
        // Arrange
        await using var testContext = CreateContext();
        var odd = RenderDropzone(testContext, TicketParity.Odd);
        var sut = new SampleDropPolicy();

        // Act
        var accepted = sut.Accepts(Substitute.For<IDraggable>(), odd);

        // Assert
        accepted.Should().BeTrue();
    }

    private static BunitContext CreateContext()
    {
        var testContext = new BunitContext();
        testContext.Services
            .AddScoped(_ => Substitute.For<IDragInteraction>())
            .AddScoped(_ => Substitute.For<ISnapToGridPointerCaptureBehavior>())
            .AddScoped(_ => Substitute.For<IDropPolicy<IDropzone>>())
            .AddScoped(_ => Substitute.For<IDropHandler<IDropzone>>());

        return testContext;
    }

    private static TicketDispenser RenderDispenser(BunitContext testContext)
        => testContext.Render<TicketDispenser>(b => b.Add(p => p.HandOutDuration, TimeSpan.Zero)).Instance;

    private static Dropzone RenderDropzone(BunitContext testContext, TicketParity servedTickets)
        => testContext.Render<Dropzone>(b => b
            .Add(p => p.Label, servedTickets.ToString())
            .Add(p => p.ServedTickets, servedTickets)).Instance;
}
