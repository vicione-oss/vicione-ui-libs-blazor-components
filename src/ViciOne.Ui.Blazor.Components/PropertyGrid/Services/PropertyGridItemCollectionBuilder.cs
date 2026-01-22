using ViciOne.Ui.Blazor.Components.PropertyGrid.Factories;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;
using TargetInstanceType = System.Type;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyGridItemCollectionBuilder<TContext>(
    IEnumerable<IPropertyDescriptorProvider<TContext>> propertyDescriptorProviders,
    IPropertyValueEqualityComparerProvider<TContext> propertyValueEqualityComparerProvider)
        : IPropertyGridItemCollectionBuilder<TContext>
{
    private ILookup<TargetInstanceType, IPropertyDescriptorProvider<TContext>>? _propertyDescriptorProviderLookup;

    public IReadOnlyCollection<IPropertyGridItem> Build(IEnumerable<object> instances, TContext context,
        IPropertyGridMessageStore messageStore)
    {
        var result = new Dictionary<string, IPropertyGridItem>();

        var instancesByType = instances.ToLookup(i => i.GetType());

        if (instancesByType.Count == 0)
            return [];

        var commonProperties = FindCommonProperties(instancesByType, context);
        if (commonProperties is null)
            return [];

        foreach (var commonProperty in commonProperties)
        {
            var valueType = commonProperty.Key.ValueType;

            var propertyDescriptors = commonProperty.Value;

            var valueComparer = propertyValueEqualityComparerProvider.GetPropertyValueEqualityComparer(valueType);

            IPropertyGridItem? propertyGridItem = null;

            if (commonProperty.Key is ICommonNumericPropertyKey commonNumericPropertyKey)
            {
                var createDelegate = CreateNumericPropertyGridItem<int, int, int>;

                var createMethodInfo = createDelegate.Method
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(commonNumericPropertyKey.ValueType, commonNumericPropertyKey.IntervalType,
                        commonNumericPropertyKey.LimitType);

                object?[] parameters = [instancesByType, propertyDescriptors, valueComparer, messageStore, commonNumericPropertyKey];

                propertyGridItem = createMethodInfo.Invoke(null, parameters) as IPropertyGridItem;
            }
            else if (commonProperty.Key is ICommonSelectionPropertyKey commonPossibleValuesPropertyKey)
            {
                var createDelegate = CreateSelectionPropertyGridItem<object>;

                var createMethodInfo = createDelegate.Method
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(commonPossibleValuesPropertyKey.ValueType);

                object?[] parameters = [instancesByType, propertyDescriptors, valueComparer, messageStore];

                propertyGridItem = createMethodInfo.Invoke(null, parameters) as IPropertyGridItem;
            }
            else if (commonProperty.Key is ICommonPropertyKey commonMishMashPropertyKey)
            {
                var createDelegate = CreatePropertyGridItem<object>;

                var createMethodInfo = createDelegate.Method
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(commonMishMashPropertyKey.ValueType);

                object?[] parameters = [instancesByType, propertyDescriptors, valueComparer, messageStore];

                propertyGridItem = createMethodInfo.Invoke(null, parameters) as IPropertyGridItem;
            }

            if (propertyGridItem is not null)
            {
                result.TryAdd(propertyGridItem.Name, propertyGridItem);
            }
        }

        return result.Values;
    }

    private static IEnumerable<IPropertyDescriptor> GetPropertyDescriptors(
        IPropertyDescriptorProvider propertyDescriptorProvider, TContext context)
    {
        var methodDelegate = GetPropertyDescriptors<TargetInstanceType>;

        var methodInfo = methodDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(propertyDescriptorProvider.GetTargetInstanceType());

        var propertyDescriptors = methodInfo.Invoke(null, [propertyDescriptorProvider, context]) as IEnumerable<IPropertyDescriptor>;

        return propertyDescriptors ?? [];
    }

    private static IEnumerable<IPropertyDescriptor<TInstance>> GetPropertyDescriptors<TInstance>(
        IPropertyDescriptorProvider<TContext, TInstance> propertyDescriptorProvider, TContext context)
            => propertyDescriptorProvider.GetPropertyDescriptors(context);

    private static bool ShouldConsiderPropertyDescriptor(IPropertyDescriptor propertyDescriptor, IEnumerable<object> instances)
    {
        var shouldConsiderPropertyDescriptorDelegate = ShouldConsiderPropertyDescriptor<TargetInstanceType>;
        var shouldConsiderPropertyDescriptorMethodInfo = shouldConsiderPropertyDescriptorDelegate.Method.GetGenericMethodDefinition()
            .MakeGenericMethod(propertyDescriptor.TargetType);

        var ofTypeExtensionDelegate = Enumerable.OfType<TargetInstanceType>;

        var ofTypeExtensionMethod = ofTypeExtensionDelegate.Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(propertyDescriptor.TargetType);

        var relevantInstances = ofTypeExtensionMethod.Invoke(null, [instances]);

        var result = shouldConsiderPropertyDescriptorMethodInfo.Invoke(null, [propertyDescriptor, relevantInstances]);
        if (result is not null and bool shouldConsider)
            return shouldConsider;
        else
            return false;
    }

    private static bool ShouldConsiderPropertyDescriptor<TInstance>(
        IPropertyDescriptor<TInstance> propertyDescriptor, IEnumerable<TInstance> instances)
    {
        if (propertyDescriptor.ConsiderPredicate is not null)
            return instances.All(propertyDescriptor.ConsiderPredicate);
        else
            return true;
    }

    private Dictionary<ICommonPropertyKey, List<IPropertyDescriptor>>? FindCommonProperties(
        ILookup<TargetInstanceType, object> instancesByType, TContext context)
    {
        _propertyDescriptorProviderLookup ??= propertyDescriptorProviders.ToLookup(k => k.GetTargetInstanceType());

        var result = new Dictionary<ICommonPropertyKey, List<IPropertyDescriptor>>();

        foreach (var instanceGrouping in instancesByType)
        {
            if (instanceGrouping == instancesByType.First())
            {
                // process first grouping to create a baseline
                var targetInstanceType = instanceGrouping.Key;
                var targetInstances = instanceGrouping;

                var propertyDescriptorProviders = _propertyDescriptorProviderLookup[targetInstanceType];

                var propertyDescriptors = propertyDescriptorProviders
                    .SelectMany(propertyDescriptorProvider => GetPropertyDescriptors(propertyDescriptorProvider, context))
                    .Where(propertyDescriptor => ShouldConsiderPropertyDescriptor(propertyDescriptor, targetInstances))
                    .ToList();

                foreach (var propertyDescriptor in propertyDescriptors)
                {
                    var commonPropertyKey = CommonPropertyKeyFactory.CreateCommonPropertyKey(propertyDescriptor);

                    if (!result.TryGetValue(commonPropertyKey, out var consideredPropertyDescriptors))
                    {
                        consideredPropertyDescriptors = [];

                        result.Add(commonPropertyKey, consideredPropertyDescriptors);
                    }

                    consideredPropertyDescriptors.Add(propertyDescriptor);
                }
            }
            else
            {
                var targetInstanceType = instanceGrouping.Key;
                var targetInstances = instanceGrouping;

                var propertyDescriptorProviders = _propertyDescriptorProviderLookup[targetInstanceType];

                var propertyDescriptorLookup = propertyDescriptorProviders
                    .SelectMany(propertyDescriptorProvider => GetPropertyDescriptors(propertyDescriptorProvider, context))
                    .Where(propertyDescriptor => ShouldConsiderPropertyDescriptor(propertyDescriptor, targetInstances))
                    .ToLookup(CommonPropertyKeyFactory.CreateCommonPropertyKey);

                foreach (var commonPropertyKey in result.Keys.ToArray())
                {
                    // if common property key is not associated with any fetched property descriptor 
                    if (!propertyDescriptorLookup.Contains(commonPropertyKey))
                        result.Remove(commonPropertyKey); // ... then the key does not represent a common property
                }

                foreach (var propertyDescriptorGrouping in propertyDescriptorLookup)
                {
                    var commonPropertyKey = propertyDescriptorGrouping.Key;

                    if (result.TryGetValue(commonPropertyKey, out var associatedPropertyDescriptor))
                    {
                        associatedPropertyDescriptor.AddRange(propertyDescriptorGrouping);
                    }
                }
            }
        }

        return result;
    }

    private static PropertyGridItem<TPropertyValue> CreatePropertyGridItem<TPropertyValue>(
        ILookup<TargetInstanceType, object> instancesByType, IEnumerable<IPropertyDescriptor> propertyDescriptors,
        IEqualityComparer<TPropertyValue> valueEqualityComparer, IPropertyGridMessageStore messageStore)
    {
        var result = new PropertyGridItem<TPropertyValue>(instancesByType, propertyDescriptors,
            valueEqualityComparer, messageStore);

        return result;
    }

    private static SelectionPropertyGridItem<TPropertyValue> CreateSelectionPropertyGridItem<TPropertyValue>(
        ILookup<TargetInstanceType, object> instancesByType, IEnumerable<IPropertyDescriptor> propertyDescriptors,
        IEqualityComparer<TPropertyValue> valueEqualityComparer, IPropertyGridMessageStore messageStore)
    {
        var result = new SelectionPropertyGridItem<TPropertyValue>(instancesByType, propertyDescriptors,
            valueEqualityComparer, messageStore);

        return result;
    }

    private static NumericPropertyGridItem<TPropertyValue, TInterval, TLimit>
        CreateNumericPropertyGridItem<TPropertyValue, TInterval, TLimit>(
            ILookup<TargetInstanceType, object> instancesByType, IEnumerable<IPropertyDescriptor> propertyDescriptors,
            IEqualityComparer<TPropertyValue> valueEqualityComparer, IPropertyGridMessageStore messageStore,
            ICommonNumericPropertyKey<TInterval, TLimit> propertyKey)
                where TInterval : struct
                where TLimit : struct
    {
        var result = new NumericPropertyGridItem<TPropertyValue, TInterval, TLimit>(instancesByType, propertyDescriptors,
            valueEqualityComparer, messageStore, propertyKey.Interval, propertyKey.IsRastered,
            propertyKey.Minimum, propertyKey.Maximum);

        return result;
    }
}
