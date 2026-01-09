using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Services;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for core functionality in context menus
    /// </summary>
    public static IServiceCollection AddContextMenuCore(this IServiceCollection services)
    {
        services.AddPopup();

        services.TryAddScoped<IContextMenuSettings, ContextMenuSettings>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IContextMenuRequest{TContext}"/> in DI container
    /// </summary>
    public static IServiceCollection AddContextMenuRequest<TContext>(this IServiceCollection services, object? serviceKey = null)
        where TContext : class, IContextMenuContext
    {
        services.TryAddKeyedScoped<IContextMenuRequest<TContext>, ContextMenuRequest<TContext>>(serviceKey);

        return services;
    }

    /// <summary>
    /// Registers <see cref="IContextMenuState{TContext}"/> in DI container
    /// </summary>
    public static IServiceCollection AddContextMenuState<TContext, TStateClass>(this IServiceCollection services, object? serviceKey = null)
        where TContext : class, IContextMenuContext
        where TStateClass : class, IContextMenuState<TContext>
    {
        services.TryAddKeyedScoped<TStateClass>(serviceKey);

        return services;
    }
}
