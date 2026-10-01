using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace Shared.Pages.Draggable.Components;

/// <summary>
/// Accepts every draggable on every dropzone, except a ticket: only a dropzone serving it accepts that.
/// </summary>
internal sealed class SampleDropPolicy : IDropPolicy<IDropzone>
{
    public bool Accepts(IDraggable draggable, IDropzone target)
        // Asked at drag start. The dispenser has handed out this drag's ticket by then, not still the previous one.
        => draggable is not TicketDispenser { Ticket: { } ticket }
            || target is not Dropzone { ServedTickets: { } servedTickets }
            || servedTickets == (ticket % 2 == 1 ? TicketParity.Odd : TicketParity.Even);
}
