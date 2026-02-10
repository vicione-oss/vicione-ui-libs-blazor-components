using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace ViciOne.Ui.Blazor.Components.PointerCapture.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ISnapToGridPointerCaptureBehavior"/> to <paramref name="services"/>.
    /// </summary>
    public static IServiceCollection AddSnapToGridPointerCaptureBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<ISnapToGridPointerCaptureBehavior, SnapToGridPointerCaptureBehavior>();

        return services;
    }
}
