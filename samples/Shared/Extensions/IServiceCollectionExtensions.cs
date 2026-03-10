using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Breadcrumb.Extensions;
using Shared.Pages.CheckBox.Extensions;
using Shared.Pages.ContextMenu.Extensions;
using Shared.Pages.ExpandableMenu.Extensions;
using Shared.Pages.Grid.Extensions;
using Shared.Pages.Moveable.Extensions;
using Shared.Pages.PropertyGrid.Extensions;
using Shared.Pages.SectionRail.Extensions;
using Shared.Pages.SpinEdit.Extensions;
using Shared.Pages.Toolbar.Extensions;
using Shared.Pages.ToolTip.Extensions;

namespace Shared.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        services.AddBreadcrumbPage()
            .AddCheckBoxPage()
            .AddContextMenuPage()
            .AddExpandableMenuPage()
            .AddGridPage()
            .AddMoveablePage()
            .AddPropertyGridPage()
            .AddSectionRailPage()
            .AddSpinEditPage()
            .AddToolbarPage()
            .AddTooltipPage();

        return services;
    }
}
