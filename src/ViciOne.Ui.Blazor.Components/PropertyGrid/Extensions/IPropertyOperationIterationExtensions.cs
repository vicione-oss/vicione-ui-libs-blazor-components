using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

internal static class IPropertyOperationIterationExtensions
{
    private static bool ReadUnified(this IReadOnlyList<PropertyOperationIteration> iterations,
        Func<IPropertyDescriptor<object>, IReadOnlyList<object>, bool> @delegate, bool defaultValue)
    {
        foreach (var i in iterations)
        {
            var method = @delegate.Method.GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances };
            var result = method.Invoke(null, parameters);
            if (result is bool value)
            {
                if (value != defaultValue)
                    return value;
            }
            else
            {
                return !defaultValue;
            }
        }

        return defaultValue;
    }

    private static bool ReadUnified(this IReadOnlyList<PropertyOperationIteration> iterations,
        Func<IPropertyDescriptor<object, object>, IReadOnlyList<object>, bool> @delegate, bool defaultValue)
    {
        foreach (var i in iterations)
        {
            var method = @delegate.Method.GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances };
            var result = method.Invoke(null, parameters);
            if (result is bool value)
            {
                if (value != defaultValue)
                    return value;
            }
            else
            {
                return !defaultValue;
            }
        }

        return defaultValue;
    }

    public static bool ReadUnifiedCanSetValue(this IReadOnlyList<PropertyOperationIteration> iterations)
        => iterations.ReadUnified(@delegate: IPropertyDescriptorExtensions.CanSetValue, defaultValue: true);

    public static bool ReadUnifiedEnabled(this IReadOnlyList<PropertyOperationIteration> iterations)
        => iterations.ReadUnified(@delegate: IPropertyDescriptorExtensions.ReadUnifiedEnabled, defaultValue: true);

    public static bool ReadUnifiedReadOnly(this IReadOnlyList<PropertyOperationIteration> iterations)
        => iterations.ReadUnified(@delegate: IPropertyDescriptorExtensions.ReadUnifiedReadOnly, defaultValue: false);

    public static bool ReadUnifiedVisible(this IReadOnlyList<PropertyOperationIteration> iterations)
        => iterations.ReadUnified(@delegate: IPropertyDescriptorExtensions.ReadUnifiedVisible, defaultValue: true);

    public static bool ReadUnifiedResettable(this IReadOnlyList<PropertyOperationIteration> iterations)
        => iterations.ReadUnified(@delegate: IPropertyDescriptorExtensions.ReadUnifiedResettable, defaultValue: true);

    public static List<ISelectableValue<TPropertyValue>>? ReadUnifiedSelectableValues<TPropertyValue>(
        this IReadOnlyList<PropertyOperationIteration> iterations,
        IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var result = new List<ISelectableValue<TPropertyValue>>();

        var differentValueFound = false;

        foreach (var i in iterations)
        {
            var readUnifiedSelectableValuesDelegate = IPropertyDescriptorExtensions.ReadUnifiedSelectableValues<object, TPropertyValue>;
            var readUnifiedSelectableValuesMethod = readUnifiedSelectableValuesDelegate.Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var invokeResult = readUnifiedSelectableValuesMethod.Invoke(null,
                [i.PropertyDescriptor, i.Instances, valueEqualityComparer]);

            if (invokeResult is null)
            {
                return null;
            }
            else if (invokeResult is List<ISelectableValue<TPropertyValue>> currentUnifiedPossibleValues)
            {
                if (i == iterations[0])
                {
                    result.AddRange(currentUnifiedPossibleValues);

                    continue;
                }

                var differentValues = result.Except(currentUnifiedPossibleValues, valueEqualityComparer).ToList();

                foreach (var differentValue in differentValues)
                {
                    result.Remove(differentValue);

                    differentValueFound = true;
                }
            }
        }

        if (differentValueFound && result.Count == 0)
            return null;

        return result;
    }

    public static ValueOf<TPropertyValue>? ReadUnifiedDefaultValue<TPropertyValue>(
        this IReadOnlyList<PropertyOperationIteration> iterations, IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var tryReadUnifiedDelegate = IPropertyDescriptorExtensions.ReadDefaultValue<object, TPropertyValue>;

        return iterations.ReadUnified(valueEqualityComparer, tryReadUnifiedDelegate);
    }

    public static ValueOf<TPropertyValue>? ReadUnifiedValue<TPropertyValue>(
        this IReadOnlyList<PropertyOperationIteration> iterations, IEqualityComparer<TPropertyValue> valueEqualityComparer)
    {
        var tryReadUnifiedDelegate = IPropertyDescriptorExtensions.ReadUnifiedValue<object, TPropertyValue>;

        return iterations.ReadUnified(valueEqualityComparer, tryReadUnifiedDelegate);
    }

    public static bool? ReadUnifiedHasValueDifferentFromDefaultValue<TPropertyValue>(
        this IReadOnlyList<PropertyOperationIteration> iterations, TPropertyValue defaultValue)
    {
        var readDelegate = IPropertyDescriptorExtensions.ReadUnifiedHasValueDifferentFromDefaultValue<object, TPropertyValue>;

        bool? previousValue = null;

        foreach (var i in iterations)
        {
            var method = readDelegate.Method.GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var invokeResult = method.Invoke(null, [i.PropertyDescriptor, i.Instances, defaultValue]);
            if (invokeResult is bool currentValue)
            {
                if (previousValue is null)
                {
                    previousValue = currentValue;

                    continue;
                }

                if (currentValue != previousValue)
                    return null;
            }
            else
            {
                return null;
            }
        }

        return previousValue;
    }

    private static ValueOf<TPropertyValue>? ReadUnified<TPropertyValue>(
        this IReadOnlyList<PropertyOperationIteration> iterations, IEqualityComparer<TPropertyValue> valueEqualityComparer,
        Func<IPropertyDescriptor<object, TPropertyValue>, IReadOnlyList<object>,
            IEqualityComparer<TPropertyValue>, ValueOf<TPropertyValue>?> tryReadUnifiedDelegate)
    {
        ValueOf<TPropertyValue>? previousValue = null;

        foreach (var i in iterations)
        {
            var tryReadUnifiedMethod = tryReadUnifiedDelegate.Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var invokeResult = tryReadUnifiedMethod.Invoke(null, [i.PropertyDescriptor, i.Instances, valueEqualityComparer]);
            if (invokeResult is ValueOf<TPropertyValue> currentValue)
            {
                if (previousValue is null)
                {
                    previousValue = currentValue;

                    continue;
                }

                if (!valueEqualityComparer.Equals(currentValue.Value, previousValue.Value))
                    return null;
            }
            else
            {
                return null;
            }
        }

        return previousValue;
    }

    public static SetValueResult SetValue<TPropertyValue>(this IReadOnlyList<PropertyOperationIteration> iterations,
        TPropertyValue value, out List<Exception> exceptions)
    {
        exceptions = [];

        var oldValues = new Dictionary<ValueKey, TPropertyValue>();

        // read
        foreach (var i in iterations)
        {
            var readValueDelegate = IPropertyDescriptorExtensions.FillValueMap<object, TPropertyValue>;

            var readValueMethod = readValueDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances, oldValues };
            readValueMethod.Invoke(null, parameters);
        }

        // set
        foreach (var i in iterations)
        {
            var setValueDelegate = IPropertyDescriptorExtensions.SetValueForInstances<object, TPropertyValue>;

            var setValueMethod = setValueDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances, value, exceptions };
            setValueMethod.Invoke(null, parameters);
        }

        if (exceptions.Count > 0)
        {
            // restore
            foreach (var i in iterations)
            {
                var extensionDelegate = IPropertyDescriptorExtensions.SetValueFromMap<object, TPropertyValue>;

                var extensionMethod = extensionDelegate.Method
                    .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

                var parameters = new object?[] { i.PropertyDescriptor, i.Instances, oldValues, exceptions };
                extensionMethod.Invoke(null, parameters);
            }

            return SetValueResult.SaveFailedAndValuesRestored;
        }

        return SetValueResult.Success;
    }

    public static ResetValueResult ResetValue<TPropertyValue>(this IReadOnlyList<PropertyOperationIteration> iterations,
        out List<Exception> exceptions)
    {
        exceptions = [];

        var oldValues = new Dictionary<ValueKey, TPropertyValue?>();

        // read
        foreach (var i in iterations)
        {
            var readValueExtensionDelegate = IPropertyDescriptorExtensions.FillValueMap<object, TPropertyValue>;

            var readValueExtensionMethod = readValueExtensionDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances, oldValues };
            readValueExtensionMethod.Invoke(null, parameters);
        }

        // set
        foreach (var i in iterations)
        {
            var resetValueExtensionDelegate = IPropertyDescriptorExtensions.ResetValue<object, TPropertyValue>;

            var resetValueExtensionMethod = resetValueExtensionDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances, exceptions };
            resetValueExtensionMethod.Invoke(null, parameters);
        }

        if (exceptions.Count == 0)
            return ResetValueResult.Success;

        // restore
        foreach (var i in iterations)
        {
            var extensionDelegate = IPropertyDescriptorExtensions.SetValueFromMap<object, TPropertyValue>;

            var extensionMethod = extensionDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor, i.Instances, oldValues, exceptions };
            extensionMethod.Invoke(null, parameters);
        }

        return ResetValueResult.ResetFailedAndValuesRestored;
    }

    public static void Validate<TPropertyValue>(this IReadOnlyList<PropertyOperationIteration> iterations,
        TPropertyValue value, List<string> validationMessages)
    {
        var validators = new List<IPropertyValueValidator<TPropertyValue>>();

        // collect validators
        foreach (var i in iterations)
        {
            var getValueValidatorsDelegate = IPropertyDescriptorExtensions.GetValueValidators<object, TPropertyValue>;

            var getValueValidatorsMethod = getValueValidatorsDelegate.Method
                .GetGenericMethodDefinition().MakeGenericMethod(i.InstanceType, i.PropertyValueType);

            var parameters = new object?[] { i.PropertyDescriptor };
            var result = getValueValidatorsMethod.Invoke(null, parameters);
            if (result is IEnumerable<IPropertyValueValidator<TPropertyValue>> returnedValidators)
            {
                foreach (var validator in returnedValidators)
                {
                    if (!validators.Contains(validator))
                        validators.Add(validator);
                }
            }
        }

        // validate
        foreach (var validator in validators)
        {
            var validationMessage = validator.Validate(value);

            if (string.IsNullOrWhiteSpace(validationMessage) || validationMessages.Contains(validationMessage))
                continue;

            validationMessages.Add(validationMessage);
        }
    }
}
