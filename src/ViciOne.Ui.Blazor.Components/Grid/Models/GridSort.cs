using System.Linq.Expressions;
using QuickGrid = Microsoft.AspNetCore.Components.QuickGrid;

namespace ViciOne.Ui.Blazor.Components.Grid.Models;

/// <inheritdoc cref="QuickGrid.GridSort{TGridItem}"/>
public sealed class GridSort<TGridItem>
{
    private readonly QuickGrid.GridSort<TGridItem>? _gridSort;

    private GridSort(QuickGrid.GridSort<TGridItem> gridSort)
        => _gridSort = gridSort;

    /// <inheritdoc cref="QuickGrid.GridSort{TGridItem}.ByAscending{U}(Expression{Func{TGridItem, U}})"/>
    public static GridSort<TGridItem> ByAscending<T>(Expression<Func<TGridItem, T>> expression)
        => new(QuickGrid.GridSort<TGridItem>.ByAscending(expression));

    /// <inheritdoc cref="QuickGrid.GridSort{TGridItem}.ByAscending{U}(Expression{Func{TGridItem, U}})"/>
    public static GridSort<TGridItem> ByDescending<T>(Expression<Func<TGridItem, T>> expression)
        => new(QuickGrid.GridSort<TGridItem>.ByDescending(expression));

    internal QuickGrid.GridSort<TGridItem>? ToQuickGridSort()
        => _gridSort;
}
