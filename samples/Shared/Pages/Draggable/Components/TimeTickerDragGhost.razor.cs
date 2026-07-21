using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;
using ViciOne.Ui.Blazor.Components.Draggable.Components;

namespace Shared.Pages.Draggable.Components;

public sealed partial class TimeTickerDragGhost
    : DragGhostBase, IDragStartListener, IDragEndListener, IDropzoneEnterListener, IDropzoneLeaveListener
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));

    private string? _stateCssModifier;
    private string? _sizeCssModifier;
    private int _tickCount;
    private DateTime _currentTime = DateTime.Now;

    [Inject] private ILogger<TimeTickerDragGhost> Logger { get; set; } = default!;

    protected override void OnInitialized()
        => _ = TickAsync();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _timer.Dispose();

        base.Dispose(disposing);
    }

    private async Task TickAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync())
            {
                _currentTime = DateTime.Now;

                // Every 3 seconds toggle the ghost's size (larger icon and font). The differently-sized
                // content is swapped into the ghost, exercising the re-anchor behavior that keeps the
                // grabbed fraction under the pointer at the new size.
                if (++_tickCount % 3 == 0)
                    _sizeCssModifier = _sizeCssModifier is null ? "large" : null;

                // Re-render the ghost content. This runs outside a lifecycle callback; RenderContentAsync
                // completes the JS ghost's pending content-change wait, keeping the clock ticking while it is
                // being dragged.
                await InvokeAsync(RenderContentAsync);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the timer is disposed.
        }
    }

    public async Task DragStartAsync()
    {
        Logger.LogInformation("Drag start recognized.");

        _stateCssModifier = "outside-dropzone";

        await RenderContentAsync();
    }

    public async Task DragEndAsync()
    {
        Logger.LogInformation("Drag end recognized.");

        _stateCssModifier = null;

        await RenderContentAsync();
    }

    public async Task DropzoneEnterAsync()
    {
        Logger.LogInformation("Dropzone enter recognized.");

        _stateCssModifier = "over-dropzone";

        await RenderContentAsync();
    }

    public async Task DropzoneLeaveAsync()
    {
        Logger.LogInformation("Dropzone leave recognized.");

        _stateCssModifier = "outside-dropzone";

        await RenderContentAsync();
    }
}
