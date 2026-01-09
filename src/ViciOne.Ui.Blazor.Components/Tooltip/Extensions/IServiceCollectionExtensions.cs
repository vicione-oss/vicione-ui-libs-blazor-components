using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Components;
using ViciOne.Ui.Blazor.Components.Tooltip.Services;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="TooltipContainer"/> and <see cref="TooltipDisplay"/>
    /// </summary>
    public static IServiceCollection AddTooltip(this IServiceCollection services)
    {
        services.TryAddScoped<TooltipService>();

        return services;
    }
}
