using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

internal sealed class InsertionOrderPropertyComparer : IInsertionOrderPropertyComparer
{
    public int Compare(IPropertyGridItem? x, IPropertyGridItem? y)
        => 0;
}
