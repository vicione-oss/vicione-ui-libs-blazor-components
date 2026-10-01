namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;

internal interface ISimpleTable<TItem>
    where TItem : class
{
    /// <summary>
    /// Every row the table is backed by, before any filter.
    /// </summary>
    IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// The rows the active filter leaves, across the whole table and independent of any
    /// virtualization window. Empty until the first provide has run. Bulk selection and the select-all
    /// tri-state act on this, never on <see cref="Items"/>, so rows a filter hides are never touched by a
    /// bulk action.
    /// </summary>
    IReadOnlyList<TItem> FilteredItems { get; }
}
