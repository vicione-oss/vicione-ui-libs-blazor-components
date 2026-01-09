using Microsoft.AspNetCore.Components;
using Shared.Models;
using Shared.Services;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Components;

public sealed partial class SecondContextMenu
    : SpecializedContextMenuWithStateBase<SecondContextMenuContext, SecondContextMenuState>
{
    private readonly string _editIcon = MonochromeIconName.Edit.GetCssClasses(MonochromeIconSize.Small).ToSpaceSeparated();

    [Parameter]
    public EventCallback MenuItemClicked { get; set; }

    [Parameter]
    public EventCallback<bool> VisibilityChanged { get; set; }

    private async Task ContextMenuVisibilityChangedAsync(bool isVisible)
    {
        if (VisibilityChanged.HasDelegate)
            await VisibilityChanged.InvokeAsync(isVisible);
    }

    private async Task OnItemClickedAsync()
    {
        if (MenuItemClicked.HasDelegate)
            await MenuItemClicked.InvokeAsync();
    }
}
