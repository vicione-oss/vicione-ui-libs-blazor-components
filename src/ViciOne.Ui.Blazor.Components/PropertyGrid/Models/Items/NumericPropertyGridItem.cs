using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

internal sealed class NumericPropertyGridItem<TPropertyValue, TInterval, TLimit>(ILookup<Type, object> instancesByType,
    IEnumerable<IPropertyDescriptor> propertyDescriptors, IEqualityComparer<TPropertyValue> valueEqualityComparer,
    IPropertyGridMessageStore messageStore, TInterval interval, bool isRasteredValue, TLimit minimum, TLimit maximum)
        : PropertyGridItem<TPropertyValue>(instancesByType, propertyDescriptors, valueEqualityComparer, messageStore),
            INumericPropertyGridItem<TInterval, TLimit>
                where TInterval : struct
                where TLimit : struct
{
    public TInterval Interval => interval;
    public bool IsRasteredValue => isRasteredValue;
    public TLimit Minimum => minimum;
    public TLimit Maximum => maximum;

    public object GetIntervalBoxed() => interval;
    public object GetMinimumBoxed() => minimum;
    public object GetMaximumBoxed() => maximum;
}
