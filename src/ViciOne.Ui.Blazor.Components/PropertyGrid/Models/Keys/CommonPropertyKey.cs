namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal readonly record struct CommonPropertyKey(string Category, string Name, Type ValueType)
    : ICommonPropertyKey;
