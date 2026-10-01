namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Comparers;

/// <summary>
/// Builds the comparer a table resolves row identity with.
/// </summary>
internal static class ItemIdComparer
{
    /// <summary>
    /// The comparer for <paramref name="itemIdSelector"/>: identity by the key it returns, or the item type's
    /// own equality when no selector was supplied.
    /// </summary>
    /// <remarks>
    /// Keeping both cases here means a caller never has to spell out the fallback, and so cannot accidentally
    /// pick a different one.
    /// </remarks>
    public static IEqualityComparer<TItem> For<TItem>(Func<TItem, object>? itemIdSelector)
        where TItem : class
    {
        if (itemIdSelector is null)
            return EqualityComparer<TItem>.Default;

        return new ItemIdComparer<TItem>(itemIdSelector);
    }
}
