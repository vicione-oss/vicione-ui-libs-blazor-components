using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Breadcrumb.Extensions;
using Shared.Pages.CheckBox.Extensions;
using Shared.Pages.ContextMenu.Extensions;
using Shared.Pages.Dialog.Extensions;
using Shared.Pages.Draggable.Extensions;
using Shared.Pages.ExpandableMenu.Extensions;
using Shared.Pages.Grid.Extensions;
using Shared.Pages.Moveable.Extensions;
using Shared.Pages.Popup.Extensions;
using Shared.Pages.PropertyGrid.Extensions;
using Shared.Pages.Resizeable.Extensions;
using Shared.Pages.SectionRail.Extensions;
using Shared.Pages.Sidebar.Extensions;
using Shared.Pages.SpinEdit.Extensions;
using Shared.Pages.Tables.AdvancedTable.Extensions;
using Shared.Pages.Tables.SimpleTable.Extensions;
using Shared.Pages.ToolTip.Extensions;
using Shared.Pages.Toolbar.Extensions;

namespace Shared.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddAdvancedTablePages()
            .AddBreadcrumbPage()
            .AddCheckBoxPage()
            .AddContextMenuPage()
            .AddDraggablePage()
            .AddDialogPage()
            .AddExpandableMenuPage()
            .AddGridPage()
            .AddMoveablePage()
            .AddPopupPage()
            .AddPropertyGridPage()
            .AddResizeablePage()
            .AddSectionRailPage()
            .AddSidebarPage()
            .AddSimpleTablePages()
            .AddSpinEditPage()
            .AddToolbarPage()
            .AddTooltipPage();

        return services;
    }
}
