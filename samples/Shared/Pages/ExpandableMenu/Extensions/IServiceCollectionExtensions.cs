using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Extensions;
using ViciOne.Ui.Blazor.Components.Sidebar.Extensions;

namespace Shared.Pages.ExpandableMenu.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddExpandableMenuPage(this IServiceCollection services)
        => services.AddExpandableMenu()
            .AddSidebar();
}
