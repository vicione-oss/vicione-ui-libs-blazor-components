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
        DragStartRecognized(Logger);

        _stateCssModifier = "outside-dropzone";

        await RenderContentAsync();
    }

    public async Task DragEndAsync()
    {
        DragEndRecognized(Logger);

        _stateCssModifier = null;

        await RenderContentAsync();
    }

    public async Task DropzoneEnterAsync()
    {
        DropzoneEnterRecognized(Logger);

        _stateCssModifier = "over-dropzone";

        await RenderContentAsync();
    }

    public async Task DropzoneLeaveAsync()
    {
        DropzoneLeaveRecognized(Logger);

        _stateCssModifier = "outside-dropzone";

        await RenderContentAsync();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Drag end recognized.")]
    private static partial void DragEndRecognized(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Drag start recognized.")]
    private static partial void DragStartRecognized(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dropzone enter recognized.")]
    private static partial void DropzoneEnterRecognized(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dropzone leave recognized.")]
    private static partial void DropzoneLeaveRecognized(ILogger logger);
}
