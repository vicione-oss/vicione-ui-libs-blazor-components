using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

internal sealed class AlphabeticalPropertyComparer : IAlphabeticalPropertyComparer
{
    public int Compare(IPropertyGridItem? x, IPropertyGridItem? y)
        => StringComparer.OrdinalIgnoreCase.Compare(x?.DisplayName, y?.DisplayName);
}
