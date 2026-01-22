using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Factories;

internal sealed class CommonPropertyKeyFactory
{
    public static ICommonPropertyKey CreateCommonPropertyKey(IPropertyDescriptor p)
    {
        if (p is INumericPropertyDescriptor numericPropertyDescriptor)
        {
            var createDelegate = CreateCommonNumericPropertyKey<int, int>;
            var createMethodInfo = createDelegate.Method.GetGenericMethodDefinition()
                .MakeGenericMethod(numericPropertyDescriptor.IntervalType, numericPropertyDescriptor.LimitType);

            object?[] parameters = [p.Category, p.Name, p.ValueType, numericPropertyDescriptor];

            var createResult = createMethodInfo.Invoke(null, parameters);
            if (createResult is ICommonPropertyKey commonPropertyKey)
                return commonPropertyKey;
            else
                throw new InvalidOperationException("Creating common property key failed"); // this should never happen
        }
        else if (p is ISelectionPropertyDescriptor selectionPropertyDescriptor)
        {
            return new CommonSelectionPropertyKey
            {
                Category = p.Category,
                Name = p.Name,
                ValueType = p.ValueType
            };
        }
        else
        {
            return new CommonPropertyKey()
            {
                Category = p.Category,
                Name = p.Name,
                ValueType = p.ValueType
            };
        }
    }
    private static CommonNumericPropertyKey<TInterval, TLimit> CreateCommonNumericPropertyKey<TInterval, TLimit>(
        string category, string name, Type valueType, INumericPropertyDescriptor<TInterval, TLimit> numericPropertyDescriptor)
            where TInterval : struct
            where TLimit : struct
                => new()
                {
                    Category = category,
                    Name = name,
                    ValueType = valueType,
                    Interval = numericPropertyDescriptor.Interval,
                    Minimum = numericPropertyDescriptor.Minimum,
                    Maximum = numericPropertyDescriptor.Maximum,
                    IsRastered = numericPropertyDescriptor.IsRasteredValue
                };
}
