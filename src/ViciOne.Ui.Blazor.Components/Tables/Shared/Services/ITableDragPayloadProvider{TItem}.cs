namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

/// <summary>
/// Supplies the rows a drag carries, narrowing what the table resolved.
/// </summary>
/// <typeparam name="TItem">The type of data represented by the row.</typeparam>
public interface ITableDragPayloadProvider<TItem>
{
    /// <summary>
    /// Returns the rows the drag carries.
    /// </summary>
    /// <remarks>
    /// <para>Returning an empty list makes the drag carry nothing; a provider that throws is logged and does
    /// the same.</para>
    /// <para>The returned rows are not validated against the ones handed in — a superset, a different order and
    /// unrelated instances all reach the drop side unchanged.</para>
    /// <para>Invoked synchronously, once per drag, after a drag on an unselected row has applied and reported
    /// its click-like single-select.</para>
    /// </remarks>
    /// <param name="draggedRow">The row the drag started on. It is one of <paramref name="resolvedPayload"/> by
    /// the table's selection identity, which under a supplied <see cref="AdvancedTable.Components.AdvancedTable{TItem}.ItemIdSelector"/>
    /// need not be the same instance.</param>
    /// <param name="resolvedPayload">The rows the table settled on: the whole selection when the dragged row is
    /// selected, that single row otherwise.</param>
    IReadOnlyList<TItem> GetPayload(TItem draggedRow, IReadOnlyList<TItem> resolvedPayload);
}
