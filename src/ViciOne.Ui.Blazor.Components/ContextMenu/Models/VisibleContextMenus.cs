using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

/// <summary>
/// Context menus of one root context menu that are visible at the moment the pointer is pressed
/// </summary>
[GenerateTypeScriptClass]
public sealed class VisibleContextMenus
{
    /// <summary>
    /// HTML elements of the visible context menus
    /// </summary>
    [TypeScriptPropertyInfo(Type = "HTMLElement[]")]
    public required IReadOnlyList<ElementReference> HtmlElements { get; init; }

    /// <summary>
    /// Identifies the most recent request to show or close the root context menu
    /// </summary>
    public required int RequestVersion { get; init; }
}
