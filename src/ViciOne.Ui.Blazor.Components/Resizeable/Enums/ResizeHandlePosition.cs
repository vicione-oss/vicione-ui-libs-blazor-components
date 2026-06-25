using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Enums;

/// <summary>
/// Defines the position of a resize handle.
/// </summary>
[GenerateTypeScriptEnum]
public enum ResizeHandlePosition
{
    /// <summary>
    /// Resize handle on the top-left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Resize handle on the top edge.
    /// </summary>
    Top,

    /// <summary>
    /// Resize handle on the top-right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Resize handle on the right edge.
    /// </summary>
    Right,

    /// <summary>
    /// Resize handle on the bottom-right corner.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Resize handle on the bottom edge.
    /// </summary>
    Bottom,

    /// <summary>
    /// Resize handle on the bottom-left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Resize handle on the left edge.
    /// </summary>
    Left
}
