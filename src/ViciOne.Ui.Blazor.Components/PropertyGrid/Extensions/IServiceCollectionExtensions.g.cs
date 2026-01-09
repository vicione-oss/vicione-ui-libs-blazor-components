// Auto-generated code

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services.TypeDescriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static partial class IServiceCollectionExtensions
{
    internal static IServiceCollection AddNumericValueTypeDescriptors(this IServiceCollection services)
    {
        services.AddNumericValueTypeDescriptor<byte, ByteTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<byte?, NullableByteTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<sbyte, SignedByteTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<sbyte?, NullableSignedByteTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<ushort, UnsignedShortTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<ushort?, NullableUnsignedShortTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<uint, UnsignedIntTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<uint?, NullableUnsignedIntTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<ulong, UnsignedLongTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<ulong?, NullableUnsignedLongTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<short, ShortTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<short?, NullableShortTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<int, IntTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<int?, NullableIntTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<long, LongTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<long?, NullableLongTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<decimal, DecimalTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<decimal?, NullableDecimalTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<double, DoubleTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<double?, NullableDoubleTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<float, FloatTypeDescriptor>();
        services.AddNumericValueTypeDescriptor<float?, NullableFloatTypeDescriptor>();

        return services;
    }

    private static IServiceCollection AddNumericValueTypeDescriptor<TNumericValueType, TNumericValueTypeDescriptor>(
        this IServiceCollection services)
            where TNumericValueTypeDescriptor : class, INumericValueTypeDescriptor<TNumericValueType>
    {
        var serviceCountBefore = services.Count;

        services.TryAddScoped<INumericValueTypeDescriptor<TNumericValueType>, TNumericValueTypeDescriptor>();

        if (serviceCountBefore < services.Count)
        {
            services.AddScoped<INumericValueTypeDescriptor>(
                serviceProvider => serviceProvider.GetRequiredService<INumericValueTypeDescriptor<TNumericValueType>>());
        }

        return services;
    }
}
