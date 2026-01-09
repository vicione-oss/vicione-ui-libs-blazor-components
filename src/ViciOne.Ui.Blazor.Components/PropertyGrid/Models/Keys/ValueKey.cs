using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal readonly record struct ValueKey(IPropertyDescriptor PropertyDescriptor, object Instance);
