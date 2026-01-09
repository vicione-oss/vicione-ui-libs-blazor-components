using System.Drawing;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tooltip.Components;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Models;

/// <summary>
/// Contains the content and configuration of a tooltip.
/// </summary>
internal sealed class TooltipInfo()
{
    /// <summary>
    /// Get or set the displayed content of the tooltip.
    /// </summary>
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Get or set a condition when the tooltip gets displayed.
    /// </summary>
    public Func<bool> DisplayCondition { get; set; } = () => true;

    /// <summary>
    /// <see langword="true"/> if the tooltip is currently or can be displayed. Otherwise <see langword="false"/>.
    /// </summary>
    public bool Displaying { get; set; }

    /// <summary>
    /// <see langword="true"/> if the <see cref="TooltipContainer"/> has already been disposed. Otherwise <see langword="false"/>.
    /// </summary>
    public bool Disposed { get; set; }

    /// <summary>
    /// The unique id of this exact <see cref="TooltipInfo"/>.
    /// </summary>
    public string Id { get; } = $"tooltip_{Guid.NewGuid()}";

    /// <summary>
    /// <see langword="true"/> if the Shall be ignored for displaying. Otherwise <see langword="false"/>.
    /// </summary>
    public bool Ignore
        => Content is null || !Displaying || !DisplayCondition.Invoke();

    /// <summary>
    /// Gets or sets the size at which the tooltip shall be displayed
    /// </summary>
    public Point Position { get; set; } = Point.Empty;

    /// <summary>
    /// <see langword="true"/> if the tooltip was successfully pre-rendered. Otherwise <see langword="false"/>.
    /// </summary>
    public bool PreRendered { get; set; }

    /// <summary>
    /// Gets or sets the size of the rendered tooltip.
    /// </summary>
    public Size Size { get; set; } = Size.Empty;

    /// <summary>
    /// Resets the inner state of the <see cref="TooltipInfo"/>.
    /// </summary>
    public void Reset()
    {
        PreRendered = false;
        Displaying = false;
    }
}
