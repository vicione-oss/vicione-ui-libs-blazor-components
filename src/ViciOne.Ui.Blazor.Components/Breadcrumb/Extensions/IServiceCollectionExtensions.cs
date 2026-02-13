using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Services;
using ViciOne.Ui.Blazor.Components.Resizing.Extensions;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="Components.Breadcrumb"/>.
    /// </summary>
    public static IServiceCollection AddBreadcrumb(this IServiceCollection services)
    {
        services.AddHtmlElementHelper()
            .AddResizeObserver();

        return services;
    }

    private static IServiceCollection AddHtmlElementHelper(this IServiceCollection services)
    {
        services.TryAddScoped<IHtmlElementHelper, HtmlElementHelper>();

        return services;
    }
}
