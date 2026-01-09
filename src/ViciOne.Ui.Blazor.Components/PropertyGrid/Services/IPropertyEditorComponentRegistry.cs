using System.Diagnostics.CodeAnalysis;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

internal interface IPropertyEditorComponentRegistry
{
    bool Add(PropertyEditorComponentDescriptor descriptor, Type componentType);
    bool TryGet(PropertyEditorComponentDescriptor descriptor, [MaybeNullWhen(false)] out Type componentType);
}
