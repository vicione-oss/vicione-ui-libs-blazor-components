using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.CheckBox.Extensions;
using ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Builders;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

/// <summary>
/// IServiceCollection extensions for ViciOne.Ui.Blazor.Components.PropertyGrid
/// </summary>
public static partial class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services needed for property grid functionality to <paramref name="services"/>
    /// </summary>
    public static IPropertyGridBuilder<TContext> AddPropertyGrid<TContext>(this IServiceCollection services)
    {
        var contextMenuServiceKey = typeof(TContext);

        services.AddTooltip();

        services.AddContextMenuCore();
        services.AddContextMenuRequest<PropertyEntryContextMenuContext>(contextMenuServiceKey);
        services.AddContextMenuState<PropertyEntryContextMenuContext, PropertyEntryContextMenuState>(contextMenuServiceKey);

        services.AddCheckBox();
        services.AddSpinEditVariants();
        services.AddCategoryComparers();
        services.AddPropertyComparers();

        services.AddPropertyValueEqualityComparerProvider<TContext>();

        services.TryAddScoped<IPropertyGridMessageStore<TContext>, PropertyGridMessageStore<TContext>>();
        services.TryAddScoped<IPropertyGridItemCollectionBuilder<TContext>, PropertyGridItemCollectionBuilder<TContext>>();
        services.TryAddScoped<IPropertyGridState<TContext>, PropertyGridState<TContext>>();
        services.TryAddScoped<IPropertyGridEvents<TContext>, PropertyGridEvents<TContext>>();
        services.TryAddScoped<IPropertyGridController<TContext>, PropertyGridController<TContext>>();

        services.TryAddScoped<IPropertyEditorComponentRegistry, PropertyEditorComponentRegistry>();

        services.AddNumericValueTypeDescriptors();

        var builder = new PropertyGridBuilder<TContext>(services);

        return builder;
    }

    internal static IServiceCollection AddPropertyDescriptorProvider<TContext, TPropertyDescriptorProvider>(
        this IServiceCollection services)
            where TPropertyDescriptorProvider : class, IPropertyDescriptorProvider<TContext>
    {
        services.AddScoped<IPropertyDescriptorProvider<TContext>, TPropertyDescriptorProvider>();

        return services;
    }

    internal static IServiceCollection AddPropertyValueEqualityComparer<TContext, TPropertyValue, TEqualityComparer>(
        this IServiceCollection services)
            where TPropertyValue : allows ref struct
            where TEqualityComparer : class, IPropertyValueEqualityComparer<TPropertyValue>
    {
        var serviceKey = typeof(TContext);

        services.TryAddKeyedSingleton<IPropertyValueEqualityComparer<TPropertyValue>, TEqualityComparer>(serviceKey);

        services.AddKeyedSingleton<IPropertyValueEqualityComparer>(serviceKey,
            (serviceProvider, serviceKey) => serviceProvider
                .GetRequiredKeyedService<IPropertyValueEqualityComparer<TPropertyValue>>(serviceKey));

        return services;
    }

    internal static IServiceCollection AddPropertyValueEqualityComparerProvider<TContext>(this IServiceCollection services)
    {
        services.TryAddScoped<IPropertyValueEqualityComparerProvider<TContext>>(serviceProvider =>
        {
            var propertyValueEqualityComparerServiceKey = typeof(TContext);

            var propertyValueEqualityComparers = serviceProvider.GetKeyedServices<IPropertyValueEqualityComparer>(
                propertyValueEqualityComparerServiceKey);

            return new PropertyValueEqualityComparerProvider<TContext>(propertyValueEqualityComparers);
        });

        return services;
    }

    internal static IServiceCollection AddCategoryComparers(this IServiceCollection services)
    {
        services.AddAlphabeticalCategoryComparer();
        services.AddReverseAlphabeticalCategoryComparer();
        services.AddInsertionOrderCategoryComparer();

        return services;
    }

    internal static IServiceCollection AddPropertyComparers(this IServiceCollection services)
    {
        services.AddAlphabeticalPropertyComparer();
        services.AddReverseAlphabeticalPropertyComparer();
        services.AddInsertionOrderPropertyComparer();

        return services;
    }

    internal static IServiceCollection AddAlphabeticalCategoryComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IAlphabeticalCategoryComparer, AlphabeticalCategoryComparer>();

        return services;
    }

    internal static IServiceCollection AddReverseAlphabeticalCategoryComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IReverseAlphabeticalCategoryComparer, ReverseAlphabeticalCategoryComparer>();

        return services;
    }

    internal static IServiceCollection AddInsertionOrderCategoryComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IInsertionOrderCategoryComparer, InsertionOrderCategoryComparer>();

        return services;
    }

    internal static IServiceCollection AddAlphabeticalPropertyComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IAlphabeticalPropertyComparer, AlphabeticalPropertyComparer>();

        return services;
    }

    internal static IServiceCollection AddReverseAlphabeticalPropertyComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IReverseAlphabeticalPropertyComparer, ReverseAlphabeticalPropertyComparer>();

        return services;
    }

    internal static IServiceCollection AddInsertionOrderPropertyComparer(this IServiceCollection services)
    {
        services.TryAddSingleton<IInsertionOrderPropertyComparer, InsertionOrderPropertyComparer>();

        return services;
    }

    private static IServiceCollection AddSpinEditVariants(this IServiceCollection services)
    {
        services.AddByteSpinEdit();
        services.AddNullableByteSpinEdit();

        services.AddSignedByteSpinEdit();
        services.AddNullableSignedByteSpinEdit();

        services.AddUnsignedShortSpinEdit();
        services.AddNullableUnsignedShortSpinEdit();

        services.AddUnsignedIntSpinEdit();
        services.AddNullableUnsignedIntSpinEdit();

        services.AddUnsignedLongSpinEdit();
        services.AddNullableUnsignedLongSpinEdit();

        services.AddShortSpinEdit();
        services.AddNullableShortSpinEdit();

        services.AddIntSpinEdit();
        services.AddNullableIntSpinEdit();

        services.AddLongSpinEdit();
        services.AddNullableLongSpinEdit();

        services.AddDecimalSpinEdit();
        services.AddNullableDecimalSpinEdit();

        services.AddDoubleSpinEdit();
        services.AddNullableDoubleSpinEdit();

        services.AddFloatSpinEdit();
        services.AddNullableFloatSpinEdit();

        return services;
    }
}
