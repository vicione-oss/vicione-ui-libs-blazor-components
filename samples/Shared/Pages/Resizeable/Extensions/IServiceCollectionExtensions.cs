using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizeable.Extensions;

namespace Shared.Pages.Resizeable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddResizeablePage(this IServiceCollection services)
    {
        services.AddResizeable();

        return services;
    }
}
