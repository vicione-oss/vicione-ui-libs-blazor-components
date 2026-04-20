using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

/// <summary>
/// Sorts properties A → Z by <see cref="IPropertyGridItem.DisplayName"/> using ordinal case-insensitive comparison.
/// </summary>
public interface IAlphabeticalPropertyComparer : IComparer<IPropertyGridItem>;
