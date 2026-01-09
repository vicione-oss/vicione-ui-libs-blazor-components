using Microsoft.AspNetCore.Components;
using Shared.Models;
using Shared.Services;
using ViciOne.Ui.Blazor.Components.ContextMenu.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Components;

public sealed partial class FirstContextMenu
    : SpecializedContextMenuWithStateBase<FirstContextMenuContext, FirstContextMenuState>
{
    private static readonly MonochromeIconSize s_iconSize = MonochromeIconSize.Small;

    private readonly string _alignTopIcon = MonochromeIconName.AlignTop.GetCssClasses(s_iconSize).ToSpaceSeparated();
    private readonly string _alignBottomIcon = MonochromeIconName.AlignBottom.GetCssClasses(s_iconSize).ToSpaceSeparated();
    private readonly string _alignLeftIcon = MonochromeIconName.AlignLeft.GetCssClasses(s_iconSize).ToSpaceSeparated();
    private readonly string _alignRightIcon = MonochromeIconName.AlignRight.GetCssClasses(s_iconSize).ToSpaceSeparated();

    private readonly string _dotIconData = "data:image/svg+xml;utf8,<svg width=\"14\" height=\"14\" xmlns=\"http://www.w3.org/2000/svg\"><circle style=\"fill:white\" cx=\"7\" cy=\"7\" r=\"7\"/></svg>";
    private readonly string _editIcon = MonochromeIconName.Edit.GetCssClasses(MonochromeIconSize.Small).ToSpaceSeparated();
    private readonly string _reloadIcon = MonochromeIconName.Reload.GetCssClasses(s_iconSize).ToSpaceSeparated();

    [Parameter]
    public EventCallback<bool> VisibilityChanged { get; set; }

    [Parameter]
    public EventCallback MenuButtonClicked { get; set; }

    [Parameter]
    public EventCallback MenuItemClicked { get; set; }

    private async Task ContextMenuVisibilityChangedAsync(bool isVisible)
    {
        if (VisibilityChanged.HasDelegate)
            await VisibilityChanged.InvokeAsync(isVisible);
    }

    private async Task ContextMenuButtonClickedAsync()
    {
        if (MenuButtonClicked.HasDelegate)
            await MenuButtonClicked.InvokeAsync();
    }

    private async Task ContextMenuItemClickedAsync()
    {
        if (MenuItemClicked.HasDelegate)
            await MenuItemClicked.InvokeAsync();
    }
}
