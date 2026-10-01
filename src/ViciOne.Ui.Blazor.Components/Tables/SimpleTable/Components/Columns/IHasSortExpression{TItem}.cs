using System.Collections;
using System.Linq.Expressions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;

internal interface IHasSortExpression<TItem> : ISortableColumn
{
    /// <summary>
    /// Defines by which expression sorting by this column is done.
    /// </summary>
    Expression<Func<TItem, object>>? SortExpression { get; }

    /// <summary>
    /// Custom comparer for the sort keys produced by <see cref="SortExpression"/>; only effective when
    /// <see cref="SortExpression"/> is set. When <see langword="null"/>, the default comparison is used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies to <see cref="SimpleTable{TItem}"/>'s in-memory sorting only; a table backed by a custom
    /// items provider performs its own sorting and ignores this comparer.
    /// Derive custom comparers from <see cref="Comparer{T}"/> with the column's key type — .NET comparers
    /// such as <see cref="StringComparer"/> or those built via <c>Comparer&lt;T&gt;.Create</c> work as-is.
    /// A type implementing <see cref="IComparer{T}"/> alone is not an <see cref="IComparer"/> and cannot be
    /// handed over.
    /// </para>
    /// <para>
    /// A comparer stating a key type unrelated to the keys the <see cref="SortExpression"/> declares is a
    /// misconfiguration: it is logged and skipped, and the column is sorted with the default comparison.
    /// </para>
    /// <para>
    /// A comparer implementing the non-generic <see cref="IComparer"/> alone states no key type, so there is
    /// nothing to check it against, and it is applied to whatever keys the expression produces.
    /// </para>
    /// </remarks>
    IComparer? SortComparer { get; }
}
