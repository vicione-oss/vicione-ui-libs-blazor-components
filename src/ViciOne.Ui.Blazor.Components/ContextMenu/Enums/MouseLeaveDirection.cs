using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Enums;

/// <summary>
/// Defines directions in which the mouse pointer has left an element
/// </summary>
[GenerateTypeScriptEnum]
public enum MouseLeaveDirection
{
    /// <summary>
    /// Mouse pointer has left the element on the left side
    /// </summary>
    Left,

    /// <summary>
    /// Mouse pointer has left the element on the top side
    /// </summary>
    Top,

    /// <summary>
    /// Mouse pointer has left the element on the right side
    /// </summary>
    Right,

    /// <summary>
    /// Mouse pointer has left the element on the bottom side
    /// </summary>
    Bottom
}
