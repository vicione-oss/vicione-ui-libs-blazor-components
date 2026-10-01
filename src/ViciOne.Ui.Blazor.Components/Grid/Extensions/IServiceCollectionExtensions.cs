using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.CheckBox.Extensions;
using ViciOne.Ui.Blazor.Components.Grid.Components.Columns;
using ViciOne.Ui.Blazor.Components.Grid.Services;

namespace ViciOne.Ui.Blazor.Components.Grid.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
[Obsolete(Constants.ObsoleteMessage)]
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Add services required in connection with <see cref="ItemSelectColumn{TGridItem, TGridItemKey}"/>
    /// except <see cref="IGridItemSelection{TGridItemKey}"/>, which needs to be added manually
    /// via <see cref="AddGridItemSelection{TGridItemKey}(IServiceCollection, object?, ServiceLifetime)"/>
    /// </summary>
    public static IServiceCollection AddGridItemSelectColumn(this IServiceCollection services)
    {
        services.AddCheckBox();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IGridItemSelection{TGridItemKey}"/> in DI container
    /// </summary>
    public static IServiceCollection AddGridItemSelection<TGridItemKey>(this IServiceCollection services,
        object? serviceKey = null, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ServiceDescriptor serviceDescriptor;

        if (serviceKey is not null)
        {
            serviceDescriptor = ServiceDescriptor.DescribeKeyed(typeof(IGridItemSelection<TGridItemKey>),
                serviceKey, typeof(GridItemSelection<TGridItemKey>), lifetime);
        }
        else
        {
            serviceDescriptor = ServiceDescriptor.Describe(typeof(IGridItemSelection<TGridItemKey>),
                typeof(GridItemSelection<TGridItemKey>), lifetime);
        }

        services.TryAdd(serviceDescriptor);

        return services;
    }
}
