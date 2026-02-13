using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Resizing.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IResizeObserver"/> in DI container.
    /// </summary>
    public static IServiceCollection AddResizeObserver(this IServiceCollection services)
    {
        services.TryAddScoped<IResizeObserver, ResizeObserver>();

        return services;
    }
}
