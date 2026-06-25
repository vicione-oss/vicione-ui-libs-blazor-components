using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for implementing resizeables.
    /// </summary>
    public static IServiceCollection AddResizeable(this IServiceCollection services)
    {
        services.TryAddScoped<IResizeInteraction, ResizeInteraction>();

        return services;
    }
}
