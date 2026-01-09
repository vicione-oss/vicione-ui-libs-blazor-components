using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.ContextMenu.Models;
using Shared.Pages.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;

namespace Shared.Pages.ContextMenu.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddContextMenuPage(this IServiceCollection services)
    {
        services.AddContextMenuCore();
        services.AddContextMenuRequest<FirstContextMenuContext>();
        services.AddContextMenuState<FirstContextMenuContext, FirstContextMenuState>();
        services.AddContextMenuRequest<SecondContextMenuContext>();
        services.AddContextMenuState<SecondContextMenuContext, SecondContextMenuState>();

        return services;
    }
}
