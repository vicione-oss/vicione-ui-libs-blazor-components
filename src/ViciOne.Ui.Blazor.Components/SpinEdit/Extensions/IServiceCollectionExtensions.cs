using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static partial class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue and TLimit is of type <typeparamref name="TEnum"/> and TInterval is of type <see cref="int"/>.
    /// </summary>
    public static IServiceCollection AddEnumSpinEdit<TEnum>(this IServiceCollection services)
        where TEnum : struct, IEquatable<TEnum>, ITypeSafeEnumImplemention<TEnum>
    {
        services.AddEnumSpinEditBehavior<TEnum>();

        return services;
    }

    private static IServiceCollection AddEnumSpinEditBehavior<TEnum>(this IServiceCollection services)
        where TEnum : struct, IEquatable<TEnum>, ITypeSafeEnumImplemention<TEnum>
    {
        services.TryAddScoped<IEnumSpinBehavior<TEnum>, EnumSpinBehavior<TEnum>>();

        services.TryAddScoped<ISpinBehavior<TEnum, int, TEnum>>(
            services => services.GetRequiredService<IEnumSpinBehavior<TEnum>>());

        return services;
    }
}
