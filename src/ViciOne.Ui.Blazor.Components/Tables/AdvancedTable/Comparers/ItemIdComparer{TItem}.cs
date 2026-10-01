namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Comparers;

internal sealed class ItemIdComparer<TItem>(Func<TItem, object> itemIdSelector) : IEqualityComparer<TItem>
    where TItem : class
{
    public bool Equals(TItem? x, TItem? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        return itemIdSelector(x).Equals(itemIdSelector(y));
    }

    public int GetHashCode(TItem obj)
        => itemIdSelector(obj).GetHashCode();
}
