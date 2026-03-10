using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizing.Extensions;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="Components.Toolbar"/>.
    /// </summary>
    public static IServiceCollection AddToolbar(this IServiceCollection services)
    {
        services.AddResizeObserver();

        return services;
    }
}
