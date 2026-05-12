using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Models;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.ExpandableMenu.Components;

public sealed partial class ExpandableMenuPage : ComponentBase
{
    private readonly string _hambugerMenuIconCssClass =
        MonochromeIconName.HamburgerMenu.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private readonly string _dataflowIconCssClass =
        MonochromeIconName.DataflowSolid.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private readonly string _infoOutlinedIconCssClass =
        MonochromeIconName.InfoOutlined.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private SidebarMode _sidebarMode = SidebarMode.Fluid;
    private ObservableCollection<ExpandableMenuEntry> _entries = [];

    protected override void OnInitialized()
        => _entries =
        [
            new()
            {
                ContentType = typeof(ShortDemoContent),
                IconCssClass = _hambugerMenuIconCssClass,
                Label = "File",
            },
            new()
            {
                ContentType = typeof(LongDemoContent),
                IconCssClass = _dataflowIconCssClass,
                IsDefault = true,
                Label = "Dataflow",
            },
            new()
            {
                ContentType = typeof(ShortDemoContent),
                IconCssClass = _hambugerMenuIconCssClass,
                Label = "File 2",
            },
            new()
            {
                ContentType = typeof(LongDemoContent),
                IconCssClass = _dataflowIconCssClass,
                Label = "Dataflow 2",
            },
            new()
            {
                ContentType = typeof(ShortDemoContent),
                IconCssClass = _infoOutlinedIconCssClass,
                IsSticky = true,
                Label = "Short Information (Sticky 1)",
            },
            new()
            {
                ContentType = typeof(LongDemoContent),
                IconCssClass = _infoOutlinedIconCssClass,
                IsSticky = true,
                Label = "Long Information (Sticky 2)",
            },
        ];

    private void ExpandableMenuCompactModeChanged(bool isCompact)
        => _sidebarMode = isCompact ? SidebarMode.Compact : SidebarMode.Fluid;
}
