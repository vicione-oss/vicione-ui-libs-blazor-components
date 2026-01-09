// Auto-generated code

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static partial class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="byte"/>.
    /// </summary>
    public static IServiceCollection AddByteSpinEdit(this IServiceCollection services)
    {
        services.AddByteSpinBehavior();

        return services;
    }

    private static IServiceCollection AddByteSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IByteSpinBehavior, ByteSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<byte, byte, byte>>(
            services => services.GetRequiredService<IByteSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="byte"/> and TInterval / TLimit is of type <see langword="byte"/>.
    /// </summary>
    public static IServiceCollection AddNullableByteSpinEdit(this IServiceCollection services)
    {
        services.AddNullableByteSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableByteSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableByteSpinBehavior, NullableByteSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<byte?, byte, byte>>(
            services => services.GetRequiredService<INullableByteSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="sbyte"/>.
    /// </summary>
    public static IServiceCollection AddSignedByteSpinEdit(this IServiceCollection services)
    {
        services.AddSignedByteSpinBehavior();

        return services;
    }

    private static IServiceCollection AddSignedByteSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<ISignedByteSpinBehavior, SignedByteSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<sbyte, sbyte, sbyte>>(
            services => services.GetRequiredService<ISignedByteSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="sbyte"/> and TInterval / TLimit is of type <see langword="sbyte"/>.
    /// </summary>
    public static IServiceCollection AddNullableSignedByteSpinEdit(this IServiceCollection services)
    {
        services.AddNullableSignedByteSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableSignedByteSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableSignedByteSpinBehavior, NullableSignedByteSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<sbyte?, sbyte, sbyte>>(
            services => services.GetRequiredService<INullableSignedByteSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="ushort"/>.
    /// </summary>
    public static IServiceCollection AddUnsignedShortSpinEdit(this IServiceCollection services)
    {
        services.AddUnsignedShortSpinBehavior();

        return services;
    }

    private static IServiceCollection AddUnsignedShortSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IUnsignedShortSpinBehavior, UnsignedShortSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<ushort, ushort, ushort>>(
            services => services.GetRequiredService<IUnsignedShortSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="ushort"/> and TInterval / TLimit is of type <see langword="ushort"/>.
    /// </summary>
    public static IServiceCollection AddNullableUnsignedShortSpinEdit(this IServiceCollection services)
    {
        services.AddNullableUnsignedShortSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableUnsignedShortSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableUnsignedShortSpinBehavior, NullableUnsignedShortSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<ushort?, ushort, ushort>>(
            services => services.GetRequiredService<INullableUnsignedShortSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="uint"/>.
    /// </summary>
    public static IServiceCollection AddUnsignedIntSpinEdit(this IServiceCollection services)
    {
        services.AddUnsignedIntSpinBehavior();

        return services;
    }

    private static IServiceCollection AddUnsignedIntSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IUnsignedIntSpinBehavior, UnsignedIntSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<uint, uint, uint>>(
            services => services.GetRequiredService<IUnsignedIntSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="uint"/> and TInterval / TLimit is of type <see langword="uint"/>.
    /// </summary>
    public static IServiceCollection AddNullableUnsignedIntSpinEdit(this IServiceCollection services)
    {
        services.AddNullableUnsignedIntSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableUnsignedIntSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableUnsignedIntSpinBehavior, NullableUnsignedIntSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<uint?, uint, uint>>(
            services => services.GetRequiredService<INullableUnsignedIntSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="ulong"/>.
    /// </summary>
    public static IServiceCollection AddUnsignedLongSpinEdit(this IServiceCollection services)
    {
        services.AddUnsignedLongSpinBehavior();

        return services;
    }

    private static IServiceCollection AddUnsignedLongSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IUnsignedLongSpinBehavior, UnsignedLongSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<ulong, ulong, ulong>>(
            services => services.GetRequiredService<IUnsignedLongSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="ulong"/> and TInterval / TLimit is of type <see langword="ulong"/>.
    /// </summary>
    public static IServiceCollection AddNullableUnsignedLongSpinEdit(this IServiceCollection services)
    {
        services.AddNullableUnsignedLongSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableUnsignedLongSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableUnsignedLongSpinBehavior, NullableUnsignedLongSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<ulong?, ulong, ulong>>(
            services => services.GetRequiredService<INullableUnsignedLongSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="short"/>.
    /// </summary>
    public static IServiceCollection AddShortSpinEdit(this IServiceCollection services)
    {
        services.AddShortSpinBehavior();

        return services;
    }

    private static IServiceCollection AddShortSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IShortSpinBehavior, ShortSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<short, short, short>>(
            services => services.GetRequiredService<IShortSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="short"/> and TInterval / TLimit is of type <see langword="short"/>.
    /// </summary>
    public static IServiceCollection AddNullableShortSpinEdit(this IServiceCollection services)
    {
        services.AddNullableShortSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableShortSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableShortSpinBehavior, NullableShortSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<short?, short, short>>(
            services => services.GetRequiredService<INullableShortSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="int"/>.
    /// </summary>
    public static IServiceCollection AddIntSpinEdit(this IServiceCollection services)
    {
        services.AddIntSpinBehavior();

        return services;
    }

    private static IServiceCollection AddIntSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IIntSpinBehavior, IntSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<int, int, int>>(
            services => services.GetRequiredService<IIntSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="int"/> and TInterval / TLimit is of type <see langword="int"/>.
    /// </summary>
    public static IServiceCollection AddNullableIntSpinEdit(this IServiceCollection services)
    {
        services.AddNullableIntSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableIntSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableIntSpinBehavior, NullableIntSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<int?, int, int>>(
            services => services.GetRequiredService<INullableIntSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="long"/>.
    /// </summary>
    public static IServiceCollection AddLongSpinEdit(this IServiceCollection services)
    {
        services.AddLongSpinBehavior();

        return services;
    }

    private static IServiceCollection AddLongSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<ILongSpinBehavior, LongSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<long, long, long>>(
            services => services.GetRequiredService<ILongSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="long"/> and TInterval / TLimit is of type <see langword="long"/>.
    /// </summary>
    public static IServiceCollection AddNullableLongSpinEdit(this IServiceCollection services)
    {
        services.AddNullableLongSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableLongSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableLongSpinBehavior, NullableLongSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<long?, long, long>>(
            services => services.GetRequiredService<INullableLongSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="decimal"/>.
    /// </summary>
    public static IServiceCollection AddDecimalSpinEdit(this IServiceCollection services)
    {
        services.AddDecimalSpinBehavior();

        return services;
    }

    private static IServiceCollection AddDecimalSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IDecimalSpinBehavior, DecimalSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<decimal, decimal, decimal>>(
            services => services.GetRequiredService<IDecimalSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="decimal"/> and TInterval / TLimit is of type <see langword="decimal"/>.
    /// </summary>
    public static IServiceCollection AddNullableDecimalSpinEdit(this IServiceCollection services)
    {
        services.AddNullableDecimalSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableDecimalSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableDecimalSpinBehavior, NullableDecimalSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<decimal?, decimal, decimal>>(
            services => services.GetRequiredService<INullableDecimalSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="double"/>.
    /// </summary>
    public static IServiceCollection AddDoubleSpinEdit(this IServiceCollection services)
    {
        services.AddDoubleSpinBehavior();

        return services;
    }

    private static IServiceCollection AddDoubleSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IDoubleSpinBehavior, DoubleSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<double, double, double>>(
            services => services.GetRequiredService<IDoubleSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="double"/> and TInterval / TLimit is of type <see langword="double"/>.
    /// </summary>
    public static IServiceCollection AddNullableDoubleSpinEdit(this IServiceCollection services)
    {
        services.AddNullableDoubleSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableDoubleSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableDoubleSpinBehavior, NullableDoubleSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<double?, double, double>>(
            services => services.GetRequiredService<INullableDoubleSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue, TInterval and TLimit is of type <see langword="float"/>.
    /// </summary>
    public static IServiceCollection AddFloatSpinEdit(this IServiceCollection services)
    {
        services.AddFloatSpinBehavior();

        return services;
    }

    private static IServiceCollection AddFloatSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<IFloatSpinBehavior, FloatSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<float, float, float>>(
            services => services.GetRequiredService<IFloatSpinBehavior>());

        return services;
    }

    /// <summary>
    /// Adds services required for <see cref="SpinEdit{TValue, TInterval, TLimit}"/> where
    /// TValue is of type nullable <see langword="float"/> and TInterval / TLimit is of type <see langword="float"/>.
    /// </summary>
    public static IServiceCollection AddNullableFloatSpinEdit(this IServiceCollection services)
    {
        services.AddNullableFloatSpinBehavior();

        return services;
    }

    private static IServiceCollection AddNullableFloatSpinBehavior(this IServiceCollection services)
    {
        services.TryAddScoped<INullableFloatSpinBehavior, NullableFloatSpinBehavior>();

        services.TryAddScoped<ISpinBehavior<float?, float, float>>(
            services => services.GetRequiredService<INullableFloatSpinBehavior>());

        return services;
    }
}
