using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

internal sealed class SelectionPropertyGridItem<TPropertyValue>(ILookup<Type, object> instancesByType,
    IEnumerable<IPropertyDescriptor> propertyDescriptor, IEqualityComparer<TPropertyValue> valueEqualityComparer,
    IPropertyGridMessageStore messageStore)
        : PropertyGridItem<TPropertyValue>(instancesByType, propertyDescriptor, valueEqualityComparer, messageStore),
            ISelectionPropertyGridItem<TPropertyValue>
{
    public List<ISelectableValue<TPropertyValue>>? GetUnifiedSelectableValues()
        => PropertyOperationIterations.ReadUnifiedSelectableValues(ValueEqualityComparer);
}
