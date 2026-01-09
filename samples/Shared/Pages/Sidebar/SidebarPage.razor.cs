using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Sidebar.Enums;

namespace Shared.Pages.Sidebar;

public sealed partial class SidebarPage : ComponentBase
{
    private SidebarMode _leftSidebarMode = SidebarMode.GetDefaultValue();
    private SidebarMode _rigthSidebarMode = SidebarMode.GetDefaultValue();

    private int? _leftSidebarFluidWidth;

    private static void ToggleSidebarMode(ref SidebarMode sidebarMode)
    {
        if (sidebarMode == SidebarMode.Compact)
            sidebarMode = SidebarMode.Fluid;
        else
            sidebarMode = SidebarMode.Compact;
    }
}
