using Bunit;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;
using SidebarComponent = ViciOne.Ui.Blazor.Components.Sidebar.Sidebar;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Sidebar.Extensions;

/// <summary>
/// Extension methods for <see cref="BunitJSInterop"/>
/// </summary>
public static class BUnitJsInteropExtensions
{
    /// <summary>
    /// Configures handlers for all JS-interop calls a <see cref="SidebarComponent"/> makes, using loose mode.
    /// </summary>
    /// <returns>
    /// The bUnit JS-interop module associated with the resize interaction of a <see cref="SidebarComponent"/>.
    /// </returns>
    public static BunitJSModuleInterop SetupForSidebar(this BunitJSInterop jsInterop)
        => jsInterop.SetupForResizeInteraction();
}
