namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal readonly record struct CommonSelectionPropertyKey(string Category, string Name, Type ValueType)
    : ICommonSelectionPropertyKey;
