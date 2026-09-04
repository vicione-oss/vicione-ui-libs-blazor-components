using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Sidebar.Extensions;

namespace Shared.Pages.Sidebar.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSidebarPage(this IServiceCollection services)
        => services.AddSidebar();
}
