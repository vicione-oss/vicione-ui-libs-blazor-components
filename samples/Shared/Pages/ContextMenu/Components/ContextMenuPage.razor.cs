using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Pages.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;

namespace Shared.Pages.ContextMenu.Components;

public sealed partial class ContextMenuPage : ComponentBase
{
    private bool _firstContextMenuVisible;
    private bool _secondContextMenuVisible;
    private bool _contextMenuButtonClicked;
    private bool _contextMenuItemClicked;

    [Inject]
    private IContextMenuSettings ContextMenuSettings { get; set; } = default!;

    [Inject]
    private IContextMenuRequest<FirstContextMenuContext> FirstContextMenuRequest { get; set; } = default!;

    [Inject]
    private IContextMenuRequest<SecondContextMenuContext> SecondContextMenuRequest { get; set; } = default!;

    private async Task RequestFirstContextMenuAsync(MouseEventArgs args)
    {
        var context = new FirstContextMenuContext
        {
            ItemFilter = new ContextMenuItemFilter { ApplicableTo = [typeof(FirstContextMenuModel)] },
            MouseEventArgs = args,
            ObjectOpenedOn = this
        };

        await FirstContextMenuRequest.SendAsync(context);
    }

    private async Task RequestSecondContextMenuAsync(MouseEventArgs args)
    {
        var context = new SecondContextMenuContext
        {
            ItemFilter = new ContextMenuItemFilter { ApplicableTo = [typeof(SecondContextMenuModel)] },
            MouseEventArgs = args,
            ObjectOpenedOn = this
        };

        await SecondContextMenuRequest.SendAsync(context);
    }

    private void FirstContextMenuVisibilityChanged(bool isVisible)
        => _firstContextMenuVisible = isVisible;

    private void SecondContextMenuVisibilityChanged(bool isVisible)
        => _secondContextMenuVisible = isVisible;

    private void ContextMenuButtonClicked()
    {
        _contextMenuButtonClicked = true;
        _contextMenuItemClicked = false;
    }

    private void ContextMenuItemClicked()
    {
        _contextMenuButtonClicked = false;
        _contextMenuItemClicked = true;
    }
}
