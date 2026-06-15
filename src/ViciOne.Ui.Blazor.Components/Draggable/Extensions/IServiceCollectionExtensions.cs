using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace ViciOne.Ui.Blazor.Components.Draggable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for implementing drag and drop.
    /// </summary>
    public static IServiceCollection AddDraggable(this IServiceCollection services)
    {
        services.TryAddScoped<IDragInteraction, DragInteraction>();

        return services;
    }
}
