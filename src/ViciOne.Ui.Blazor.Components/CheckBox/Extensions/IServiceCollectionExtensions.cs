using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.CheckBox.Services;

namespace ViciOne.Ui.Blazor.Components.CheckBox.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds default services required for <see cref="CheckBox{TValue}"/>
    /// </summary>
    public static IServiceCollection AddCheckBox(this IServiceCollection services)
    {
        services.TryAddScoped<ICheckBoxParameterDefaults<bool>, BoolCheckBoxParameterDefaults>();
        services.TryAddScoped<ICheckBoxParameterDefaults<bool?>, NullableBoolCheckBoxParameterDefaults>();

        return services;
    }
}
