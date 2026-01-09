using System.Drawing;
using System.Timers;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tooltip.Models;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Services;

/// <summary>
/// A service to provide functionality for tooltip components.
/// </summary>
public sealed class TooltipService : IDisposable
{
    private readonly System.Timers.Timer _displayDelayTimer = new()
    {
        AutoReset = false,
        Enabled = false,
        Interval = 1000,
    };
    private readonly List<TooltipInfo> _tooltips = [];

    /// <summary>
    /// Gets the currently active tooltips.
    /// </summary>
    internal IEnumerable<TooltipInfo> Tooltips
        => _tooltips;
    /// <summary>
    /// The base z-index to use for displaying the tooltips
    /// </summary>
    internal int BaseZIndex { get; init; } = 1000000;
    /// <summary>
    /// Get or set if the tooltip is moved left and/or up depending on its real size or on thirds of the window size.
    /// </summary>
    public bool UseThirds { get; set; }

    /// <summary>
    /// Fired when the currently active tooltips changed.
    /// </summary>
    public event Action? TooltipsChanged;

    /// <summary>
    /// Fired when a tooltip was added in order to trigger the necessary pre-render.
    /// </summary>
    public event Action? PreRenderingRequested;

    /// <summary>
    /// Creates a new instance of <see cref="TooltipService"/>.
    /// </summary>
    public TooltipService()
        => _displayDelayTimer.Elapsed += OnDisplayDelayTimerElapsed;

    /// <summary>
    /// Displays the provided tooltip.
    /// </summary>
    ///
    /// <param name="info">
    /// The tooltip to display.
    /// </param>
    internal void DisplayTooltip(TooltipInfo info)
    {
        if (info.Disposed)
        {
            RemoveTooltip(info.Id);
            return;
        }

        info.Reset();

        _tooltips.Add(info);
        PreRenderingRequested?.Invoke();
        _displayDelayTimer.Start();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _displayDelayTimer.Elapsed -= OnDisplayDelayTimerElapsed;
        _displayDelayTimer.Dispose();
    }

    private void OnDisplayDelayTimerElapsed(object? _1, ElapsedEventArgs _2)
    {
        foreach (var tooltip in _tooltips.Where(dt => !dt.Displaying && dt.PreRendered))
            tooltip.Displaying = true;

        TooltipsChanged?.Invoke();
    }

    /// <summary>
    /// Processes the necessary methods in order to display the provided tooltip.
    /// </summary>
    ///
    /// <param name="info">
    /// The tooltip to process.
    /// </param>
    internal void OnPointerEnter(TooltipInfo info)
        => DisplayTooltip(info);

    /// <summary>
    /// Processes the necessary methods in order to remove the provided tooltip from displaying.
    /// </summary>
    ///
    /// <param name="info">
    /// The tooltip to process.
    /// </param>
    internal void OnPointerLeave(TooltipInfo info)
        => RemoveTooltip(info.Id);

    /// <summary>
    /// Processes the pointer move event for the provided tooltip.
    /// </summary>
    ///
    /// <param name="info">
    /// The tooltip to process.
    /// </param>
    ///
    /// <param name="e">
    /// The arguments of the originally called pointer event.
    /// </param>
    internal static void OnPointerMove(TooltipInfo info, MouseEventArgs e)
    {
        if (info.Position == Point.Empty || !info.Displaying)
        {
            info.Position = new(
                (int)Math.Round(e.ClientX),
                (int)Math.Round(e.ClientY)
            );
        }
    }

    /// <summary>
    /// Removes the tooltip from displaying.
    /// </summary>
    ///
    /// <param name="tooltipId">
    /// The unique identifier or the tooltip to remove.
    /// </param>
    public void RemoveTooltip(string tooltipId)
    {
        _tooltips.RemoveAll(tt => tt.Id == tooltipId);
        TooltipsChanged?.Invoke();
    }
}
