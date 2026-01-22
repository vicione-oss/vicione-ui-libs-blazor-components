using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.ContextMenu;

/// <summary>
/// Context menu for providing specific actions for a property entry
/// </summary>
public sealed partial class PropertyEntryContextMenu
    : SpecializedContextMenuWithStateBase<PropertyEntryContextMenuContext, PropertyEntryContextMenuState>
{
    private readonly string _resetIconCssClass = MonochromeIconName.Reload.GetCssClasses(MonochromeIconSize.Small).ToSpaceSeparated();

    /// <summary>
    /// Raised when the visibility of the context menu changes.
    /// </summary>
    /// <remarks>
    /// The event callback parameter is <see langword="true"/> when the context menu is visible, otherwise it is <see langword="false"/>.
    /// </remarks>
    [Parameter] public EventCallback<bool> VisibilityChanged { get; set; }

    private async Task ContextMenuVisibilityChangedAsync(bool visible)
        => await VisibilityChanged.InvokeAsync(visible);

    private void ResetClick()
    {
        if (Context is not null)
        {
            var success = Context.PropertyGridItem.ResetValue();
            if (success)
                Context.OnSetValue?.Invoke();
        }
    }

    private void SetToNullClick()
    {
        if (Context is not null)
        {
            var success = Context.PropertyGridItem.SetNull();
            if (success)
                Context.OnSetValue?.Invoke();
        }
    }
}
