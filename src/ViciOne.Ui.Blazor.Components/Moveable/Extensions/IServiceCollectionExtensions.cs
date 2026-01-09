using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.Moveable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for implementing moveables.
    /// </summary>
    public static IServiceCollection AddMoveable(this IServiceCollection services)
    {
        services.TryAddScoped<IMoveInteraction, MoveInteraction>();

        return services;
    }
}
