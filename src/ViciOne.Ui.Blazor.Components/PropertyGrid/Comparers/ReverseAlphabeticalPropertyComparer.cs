using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

internal sealed class ReverseAlphabeticalPropertyComparer : IReverseAlphabeticalPropertyComparer
{
    public int Compare(IPropertyGridItem? x, IPropertyGridItem? y)
        => string.CompareOrdinal(y?.DisplayName, x?.DisplayName);
}
