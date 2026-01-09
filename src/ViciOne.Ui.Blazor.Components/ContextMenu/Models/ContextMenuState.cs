using Microsoft.AspNetCore.Components.Web;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

internal class ContextMenuState
{
    public MouseEventArgs? MouseEventArgs { get; set; }
    public ContextMenuItemFilter? ItemFilter { get; set; }
    public bool ShouldBeShown { get; set; }
}
