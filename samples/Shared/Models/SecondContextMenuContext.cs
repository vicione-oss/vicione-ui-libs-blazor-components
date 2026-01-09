using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace Shared.Models;

public class SecondContextMenuContext : IContextMenuContext
{
    public ContextMenuItemFilter? ItemFilter { get; init; }
    public required MouseEventArgs MouseEventArgs { get; init; }
    public object? ObjectOpenedOn { get; init; }
}
