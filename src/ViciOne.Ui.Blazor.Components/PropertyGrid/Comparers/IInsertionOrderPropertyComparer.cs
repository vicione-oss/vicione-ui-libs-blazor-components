using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

/// <summary>
/// Preserves the original insertion order of properties (no reordering).
/// </summary>
public interface IInsertionOrderPropertyComparer : IComparer<IPropertyGridItem>;
