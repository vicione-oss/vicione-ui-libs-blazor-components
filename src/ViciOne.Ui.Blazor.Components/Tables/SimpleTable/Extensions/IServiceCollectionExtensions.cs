using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="SimpleTable{TItem}"/>.
    /// </summary>
    public static IServiceCollection AddSimpleTable(this IServiceCollection services)
    {
        services.AddAdvancedTable();

        // Registered open generic: TItem varies per SimpleTable<TItem> usage and is not known ahead of time,
        // so this resolves for any TItem without the consumer registering one per item type.
        services.TryAddScoped(typeof(ISimpleTableSortComparerResolver<>), typeof(SimpleTableSortComparerResolver<>));

        return services;
    }
}
