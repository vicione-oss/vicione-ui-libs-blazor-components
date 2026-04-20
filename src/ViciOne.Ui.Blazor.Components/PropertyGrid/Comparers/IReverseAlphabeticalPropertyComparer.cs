using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

/// <summary>
/// Sorts properties Z → A by <see cref="IPropertyGridItem.DisplayName"/> using ordinal comparison.
/// </summary>
public interface IReverseAlphabeticalPropertyComparer : IComparer<IPropertyGridItem>;
