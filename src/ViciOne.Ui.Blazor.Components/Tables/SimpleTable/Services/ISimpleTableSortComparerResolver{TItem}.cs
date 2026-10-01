using System.Collections;
using System.Linq.Expressions;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

/// <summary>
/// Resolves a column's <see cref="IHasSortExpression{TItem}.SortComparer"/> into a comparer usable against the
/// boxed keys <see cref="IHasSortExpression{TItem}.SortExpression"/> produces.
/// </summary>
internal interface ISimpleTableSortComparerResolver<TItem>
{
    /// <summary>
    /// Resolves <paramref name="sortComparer"/> for use against the keys <paramref name="sortExpression"/>
    /// produces, or <see langword="null"/> when it does not fit them.
    /// </summary>
    /// <param name="columnId">The sorted column's id, used to cache the resolution and de-dup its misconfiguration log.</param>
    /// <param name="sortComparer">The column's <see cref="IHasSortExpression{TItem}.SortComparer"/>.</param>
    /// <param name="sortExpression">The column's <see cref="IHasSortExpression{TItem}.SortExpression"/>.</param>
    Comparer<object>? Resolve(string columnId, IComparer sortComparer, Expression<Func<TItem, object>> sortExpression);
}
