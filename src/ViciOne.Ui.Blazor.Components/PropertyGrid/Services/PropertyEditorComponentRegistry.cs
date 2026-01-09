using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services.TypeDescriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal sealed class PropertyEditorComponentRegistry : IPropertyEditorComponentRegistry
{
    private readonly ConcurrentDictionary<PropertyEditorComponentDescriptor, Type> _componentTypeMap = [];

    public int UpdateLock { get; }

    public PropertyEditorComponentRegistry(IEnumerable<INumericValueTypeDescriptor> numericValueTypeDescriptors)
    {
        _componentTypeMap.TryAdd(new(ValueType: typeof(string)), typeof(StringPropertyEditor));
        _componentTypeMap.TryAdd(new(ValueType: typeof(Uri)), typeof(UriPropertyEditor));

        foreach (var supportedBooleanPropertyValueType in Constants.SupportedBooleanPropertyValueTypes)
        {
            var valueType = supportedBooleanPropertyValueType;

            var descriptor = new PropertyEditorComponentDescriptor(ValueType: valueType);
            var componentType = typeof(BoolPropertyEditor<>).MakeGenericType(valueType);

            _componentTypeMap.TryAdd(descriptor, componentType);
        }

        foreach (var numericValueTypeDescriptor in numericValueTypeDescriptors)
        {
            var valueType = numericValueTypeDescriptor.UnderlyingType;
            var nonNullableValueType = valueType.MakeNonNullableType();

            var descriptor = new PropertyEditorComponentDescriptor(ValueType: valueType);
            var componentType = typeof(NumericPropertyEditor<,,>).MakeGenericType(valueType, nonNullableValueType, nonNullableValueType);

            _componentTypeMap.TryAdd(descriptor, componentType);
        }
    }

    public bool Add(PropertyEditorComponentDescriptor descriptor, Type componentType)
        => _componentTypeMap.TryAdd(descriptor, componentType);

    public bool TryGet(PropertyEditorComponentDescriptor descriptor, [MaybeNullWhen(false)] out Type componentType)
            => _componentTypeMap.TryGetValue(descriptor, out componentType);
}
