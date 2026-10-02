using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.FocusTrap.Services;

namespace ViciOne.Ui.Blazor.Components.FocusTrap.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
internal static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for implementing focus traps.
    /// </summary>
    public static IServiceCollection AddFocusTrap(this IServiceCollection services)
    {
        services.TryAddScoped<IFocusTrap, Services.FocusTrap>();

        return services;
    }
}
