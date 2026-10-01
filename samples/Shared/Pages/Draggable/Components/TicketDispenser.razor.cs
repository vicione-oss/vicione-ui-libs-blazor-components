using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace Shared.Pages.Draggable.Components;

/// <summary>
/// A draggable that hands out the next ticket whenever a drag of it starts. Which dropzone accepts the drag
/// depends on that ticket, so it has to be handed out before any dropzone is asked.
/// </summary>
public sealed partial class TicketDispenser : ComponentBase, IDraggable, IHasLabel, IAsyncDisposable
{
    private ElementReference _elementReference;
    private bool _disposedAsync;
    private Task? _dragInteractionAttachTask;

    [Parameter]
    public bool Draggable { get; set; }

    /// <summary>How long handing out a ticket takes.</summary>
    [Parameter]
    public TimeSpan HandOutDuration { get; set; } = TimeSpan.FromMilliseconds(500);

    [Inject]
    private IDragInteraction DragInteraction { get; set; } = default!;

    /// <summary>The ticket handed out for the latest drag, or <see langword="null"/> before the first one.</summary>
    public int? Ticket { get; private set; }

    public string Label => Ticket is { } ticket ? $"Ticket {ticket}" : "Take a ticket";

    public ElementReference GetElementReference() => _elementReference;

    /// <inheritdoc/>
    public async Task PrepareDragStartAsync()
    {
        // Stands in for a server round trip. The drag ghost follows the pointer all along; only the dropzones
        // wait for the ticket before they light up.
        await Task.Delay(HandOutDuration);

        Ticket = (Ticket ?? 0) + 1;

        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Draggable)
        {
            if (_dragInteractionAttachTask is null)
            {
                _dragInteractionAttachTask = DragInteraction.AttachAsync(this);

                await _dragInteractionAttachTask;
            }
        }
        else
        {
            await RemoveDragInteractionAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedAsync, true, false))
            return;

        await RemoveDragInteractionAsync();
    }

    private async Task RemoveDragInteractionAsync()
    {
        if (_dragInteractionAttachTask?.IsCompletedSuccessfully == true)
        {
            await DragInteraction.RemoveAsync(this);

            _dragInteractionAttachTask = null;
        }
    }
}
