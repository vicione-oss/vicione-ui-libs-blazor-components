using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;

internal interface IAdvancedTable<TItem>
    where TItem : class
{
    IReadOnlyList<TItem> SelectedItems { get; }

    /// <summary>
    /// Every registered column, hidden ones included, in the order the table renders them — not the order
    /// they registered in: all left pins, then the unpinned columns, then all right pins.
    /// </summary>
    IReadOnlyList<IAdvancedTableColumn<TItem>> Columns { get; }

    SelectionMode SelectionMode { get; }

    Func<SelectionRequest<TItem>, bool>? ItemSelectionAllowed { get; }

    event Action? ItemsChanged;

    void RegisterColumn(IAdvancedTableColumn<TItem> column);

    void UnregisterColumn(IAdvancedTableColumn<TItem> column);

    int GetTotalItemCount();

    /// <summary>
    /// Whether <paramref name="item"/> is currently selected. Identity is resolved through the table's
    /// selection comparer, so callers never need comparer access of their own — a plain
    /// <c>SelectedItems.Contains</c> uses default equality and disagrees with the row whenever an
    /// <c>ItemIdSelector</c> is in use.
    /// </summary>
    bool IsSelected(TItem item);

    /// <summary>
    /// The number of <paramref name="items"/> that are currently selected. Identity is resolved
    /// through the table's selection comparer, so callers never need comparer access of their own.
    /// </summary>
    int CountSelected(IReadOnlyCollection<TItem> items);

    Task SelectAsync(params IReadOnlyCollection<TItem> items);

    Task SelectSingleAsync(TItem item);

    Task DeselectAsync(params IReadOnlyCollection<TItem> items);
}
