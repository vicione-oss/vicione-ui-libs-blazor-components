using ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

internal static class IPropertyDescriptorExtensions
{
    public static bool CanSetValue<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        IReadOnlyList<TInstance> instances)
    {
        var enabled = propertyDescriptor.ReadUnifiedEnabled(instances);
        if (!enabled)
            return false;

        var readOnly = propertyDescriptor.ReadUnifiedReadOnly(instances);
        if (readOnly)
            return false;

        return propertyDescriptor.SetValue is not null;
    }

    public static bool ReadUnifiedResettable<TInstance>(this IPropertyDescriptor<TInstance> propertyDescriptor,
        IReadOnlyList<TInstance> instances)
    {
        if (propertyDescriptor.ResetValue is null)
            return false;

        if (propertyDescriptor.Resettable is not null)
            return instances.All(propertyDescriptor.Resettable);

        return true;
    }

    public static List<ISelectableValue<TPropertyValue>>? ReadUnifiedSelectableValues<TInstance, TPropertyValue>(
        this ISelectionPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor, IReadOnlyList<TInstance> instances,
        IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var result = new List<ISelectableValue<TPropertyValue>>();

        var firstIteration = true;
        var differentValueFound = false;

        foreach (var instance in instances)
        {
            if (firstIteration)
            {
                var selectableValues = propertyDescriptor.GetSelectableValues(instance);

                result.AddRange(selectableValues);

                firstIteration = false;
                continue;
            }

            var otherInstancePossibleValues = propertyDescriptor.GetSelectableValues(instance);

            var differentValues = result.Except(otherInstancePossibleValues, valueEqualityComparer).ToList();

            foreach (var differentValue in differentValues)
            {
                result.Remove(differentValue);

                differentValueFound = true;
            }
        }

        if (differentValueFound && result.Count == 0)
            return null;

        return result;
    }

    public static ValueOf<TPropertyValue>? ReadDefaultValue<TInstance, TPropertyValue>(
        this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor, IReadOnlyList<TInstance> instances,
        IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var readCallback = propertyDescriptor.GetDefaultValue;

        if (readCallback is null)
            return null;

        return ReadUnified(instances, valueEqualityComparer, readCallback);
    }

    public static ValueOf<TPropertyValue>? ReadUnifiedValue<TInstance, TPropertyValue>(
        this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor, IReadOnlyList<TInstance> instances,
        IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var readCallback = propertyDescriptor.GetValue;

        return ReadUnified(instances, valueEqualityComparer, readCallback);
    }

    public static bool? ReadUnifiedHasValueDifferentFromDefaultValue<TInstance, TPropertyValue>(
       this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor, IReadOnlyList<TInstance> instances,
       TPropertyValue defaultValue)
    {
        var callback = propertyDescriptor.HasValueDifferentFromDefaultValue;

        if (callback is null)
            return null;

        var results = instances
            .Select(instance => callback(instance, defaultValue))
            .Distinct()
            .ToList();

        if (results.Count == 1)
            return results[0];
        else
            return null;
    }

    private static ValueOf<TPropertyValue>? ReadUnified<TInstance, TPropertyValue>(
        IReadOnlyList<TInstance> instances, IEqualityComparer<TPropertyValue> valueEqualityComparer,
        Func<TInstance, TPropertyValue> readCallback)
    {
        ValueOf<TPropertyValue>? previousValue = null;

        foreach (var instance in instances)
        {
            if (previousValue is null)
            {
                previousValue = new ValueOf<TPropertyValue>(readCallback(instance));

                continue;
            }

            var currentValue = new ValueOf<TPropertyValue>(readCallback(instance));

            if (!valueEqualityComparer.Equals(currentValue.Value, previousValue.Value))
                return null;
        }

        return previousValue;
    }

    public static void FillValueMap<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        IReadOnlyList<TInstance> instances, Dictionary<ValueKey, TPropertyValue> valueMap)
            where TInstance : notnull
    {
        foreach (var instance in instances)
        {
            var value = propertyDescriptor.GetValue(instance);

            valueMap.Add(new ValueKey { Instance = instance, PropertyDescriptor = propertyDescriptor }, value);
        }
    }

    private static void SetValueWithExceptionHandling<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        TInstance instance, TPropertyValue value, List<Exception> exceptions)
            where TInstance : notnull
    {
        try
        {
            propertyDescriptor.SetValue?.Invoke(instance, value);
        }
        catch (SetValueException exception)
        {
            exceptions.Add(exception);
        }
        catch (Exception exception)
        {
            exceptions.Add(new SetValueException(exception.Message, propertyDescriptor, instance, value, exception));
        }
    }

    public static void SetValueForInstances<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        IReadOnlyList<TInstance> instances, TPropertyValue value, List<Exception> exceptions)
            where TInstance : notnull
    {
        if (!propertyDescriptor.CanSetValue(instances))
            return;

        foreach (var instance in instances)
            propertyDescriptor.SetValueWithExceptionHandling(instance, value, exceptions);
    }

    public static void SetValueFromMap<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        IReadOnlyList<TInstance> instances, Dictionary<ValueKey, TPropertyValue> values, List<Exception> exceptions)
            where TInstance : notnull
    {
        if (!propertyDescriptor.CanSetValue(instances))
            return;

        foreach (var instance in instances)
        {
            var valueKey = new ValueKey { Instance = instance, PropertyDescriptor = propertyDescriptor };
            if (values.TryGetValue(valueKey, out var value))
                propertyDescriptor.SetValueWithExceptionHandling(instance, value, exceptions);
        }
    }

    public static void ResetValue<TInstance, TPropertyValue>(this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor,
        IReadOnlyList<TInstance> instances, List<Exception> exceptions)
            where TInstance : notnull
    {
        if (propertyDescriptor.ResetValue is null)
        {
            exceptions.Add(new ResetValueException("ResetValue is null"));

            return;
        }

        foreach (var instance in instances)
        {
            try
            {
                propertyDescriptor.ResetValue.Invoke(instance);
            }
            catch (ResetValueException exception)
            {
                exceptions.Add(exception);
            }
            catch (Exception exception)
            {
                exceptions.Add(new ResetValueException("Unexpected error occured", instance, exception));
            }
        }
    }

    public static IEnumerable<IPropertyValueValidator<TPropertyValue>>? GetValueValidators<TInstance, TPropertyValue>(
        this IPropertyDescriptor<TInstance, TPropertyValue> propertyDescriptor)
            => propertyDescriptor.ValueValidators;

    public static bool ReadUnifiedVisible<TInstance>(this IPropertyDescriptor<TInstance> propertyDescriptor,
        IReadOnlyList<TInstance> instances)
    {
        if (propertyDescriptor.Visible is null)
            return true;

        return instances.All(propertyDescriptor.Visible);
    }

    public static bool ReadUnifiedEnabled<TInstance>(this IPropertyDescriptor<TInstance> propertyDescriptor,
        IReadOnlyList<TInstance> instances)
    {
        if (propertyDescriptor.Enabled is null)
            return true;

        return instances.All(propertyDescriptor.Enabled);
    }

    public static bool ReadUnifiedReadOnly<TInstance>(this IPropertyDescriptor<TInstance> propertyDescriptor,
        IReadOnlyList<TInstance> instances)
    {
        if (propertyDescriptor.ReadOnly is null)
            return false;

        return instances.All(propertyDescriptor.ReadOnly);
    }
}
