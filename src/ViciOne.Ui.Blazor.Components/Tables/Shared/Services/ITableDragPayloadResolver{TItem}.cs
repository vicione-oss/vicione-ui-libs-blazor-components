namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

/// <summary>
/// The table-owned resolver a row asks for its drag payload.
/// </summary>
/// <typeparam name="TItem">The type of data represented by the row.</typeparam>
public interface ITableDragPayloadResolver<TItem>
{
    /// <summary>
    /// Resolves the rows the drag carries, applying the click-like selection mutation a drag on an unselected
    /// row implies.
    /// </summary>
    /// <remarks>Invoked once at drag start; the rows it returns are the ones the drop side reads.</remarks>
    Task<IReadOnlyList<TItem>> ResolvePayloadAsync(TItem draggedRow);
}
