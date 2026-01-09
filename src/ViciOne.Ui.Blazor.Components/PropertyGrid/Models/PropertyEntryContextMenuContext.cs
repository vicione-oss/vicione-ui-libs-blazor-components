using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

/// <inheritdoc cref="IContextMenuContext"/>
public sealed class PropertyEntryContextMenuContext : IContextMenuContext
{
    /// <inheritdoc/>
    public ContextMenuItemFilter? ItemFilter { get; init; }

    /// <inheritdoc/>
    public required MouseEventArgs MouseEventArgs { get; set; }

    /// <summary>
    /// Raised when a property value has been set through the context menu.
    /// </summary>
    public Action? OnSetValue { get; set; }

    /// <summary>
    /// Property grid item associated with the context menu.
    /// </summary>
    public required IPropertyGridItem PropertyGridItem { get; set; }
}
