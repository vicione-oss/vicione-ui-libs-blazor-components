using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="Components.SectionRail{TSectionIdentifier}" />
    /// </summary>
    public static IServiceCollection AddSectionRail<TSectionIdentifier>(this IServiceCollection services)
    {
        services.TryAddScoped<IEqualityComparer<TSectionIdentifier>>(_ => EqualityComparer<TSectionIdentifier>.Default);

        return services;
    }
}
