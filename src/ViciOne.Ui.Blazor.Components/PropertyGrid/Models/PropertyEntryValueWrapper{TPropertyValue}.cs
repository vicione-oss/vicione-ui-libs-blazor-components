namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

internal readonly record struct PropertyEntryValueWrapper<TPropertyValue>(bool IsUnifiedValue, TPropertyValue? Value);
