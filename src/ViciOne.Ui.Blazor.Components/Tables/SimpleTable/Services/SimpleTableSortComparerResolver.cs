using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

// Owned one per SimpleTableItemsProvider<TItem>, on the same thread-confinement guarantee as the provider
// itself: every provide runs on the renderer's synchronization context, so the per-column cache below is only
// ever touched by one thread at a time. Only the cross-instance type-compatibility cache is static and needs
// its own thread safety, since it is shared by every resolver instance of this closed generic type.
internal sealed partial class SimpleTableSortComparerResolver<TItem>(ILogger<SimpleTableSortComparerResolver<TItem>> logger)
    : ISimpleTableSortComparerResolver<TItem>
{
    // Whether a comparer type fits a sort key type never changes once both types are loaded, so the answer is
    // cached by that pair — a comparer type and sort key type stay the same across every provide and every
    // Virtualize page for a given column, and the check involves reflection either way it resolves.
    private static readonly ConcurrentDictionary<(Type ComparerType, Type SortKeyType), bool> s_comparerCompatibility = new();

    // De-dup the wrong-type misconfiguration log per (column, comparer type) so a standing misconfiguration
    // does not spam on every provide (mirrors AdvancedTable's one-shot selection-misconfiguration log).
    private readonly HashSet<(string ColumnId, Type ComparerType)> _loggedComparerMisconfigurations = [];

    // Wrapping a comparer costs a delegate and a Comparer<object> allocation, on top of the compatibility check
    // above — worth avoiding on every provide while a column keeps handing back the same SortComparer instance.
    // A misconfigured comparer resolves to null and is cached as such too, so the one-shot log stays one-shot
    // without Resolve running the check again every provide.
    private readonly Dictionary<string, (IComparer SortComparer, Comparer<object>? Resolved)> _resolvedComparers = [];

    /// <inheritdoc/>
    // A comparer that cannot take the column's keys is logged and skipped rather than applied, because the
    // wrong-type exception would surface out of the sort and kill the Blazor Circuit on a header click.
    public Comparer<object>? Resolve(string columnId, IComparer sortComparer, Expression<Func<TItem, object>> sortExpression)
    {
        if (_resolvedComparers.TryGetValue(columnId, out var cached) && cached.SortComparer == sortComparer)
            return cached.Resolved;

        var sortKeyType = GetSortKeyType(sortExpression);

        Comparer<object>? resolved = null;

        if (CanCompareSortKeys(sortComparer, sortKeyType))
            resolved = Comparer<object>.Create(sortComparer.Compare);
        else if (_loggedComparerMisconfigurations.Add((columnId, sortComparer.GetType())))
            SortComparerNotApplicable(logger, columnId, sortComparer.GetType().Name, sortKeyType.Name);

        _resolvedComparers[columnId] = (sortComparer, resolved);

        return resolved;
    }

    // A value-type key is boxed into the expression's object return, so its type sits on the Convert node's
    // operand rather than on the body. Reading the body is all there is to go on: a sort key does not have to
    // be a property — "x => !x.IsInput" is as valid as "x => x.Name".
    private static Type GetSortKeyType(Expression<Func<TItem, object>> sortExpression)
        => sortExpression.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert
            ? convert.Operand.Type
            : sortExpression.Body.Type;

    // A comparer states one key type per IComparer<T> it or a base class implements, so there can be several
    // and any one of them fitting is enough. Fitting counts in either direction: a base of the sort key type
    // fits by IComparer<T>'s own contravariance (an IComparer<object> is an IComparer<string>), and a narrower
    // type fits too, the normal shape wherever the expression produces object or an interface. Stating none at
    // all means the non-generic IComparer, which takes any object.
    private static bool CanCompareSortKeys(IComparer comparer, Type sortKeyType)
        => s_comparerCompatibility.GetOrAdd((comparer.GetType(), sortKeyType),
            static key => ComputeCanCompareSortKeys(key.ComparerType, key.SortKeyType));

    private static bool ComputeCanCompareSortKeys(Type comparerType, Type sortKeyType)
    {
        var comparerKeyTypes = comparerType.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IComparer<>))
            .Select(x => x.GetGenericArguments()[0])
            .ToList();

        return comparerKeyTypes.Count == 0 ||
            comparerKeyTypes.Any(x => x.IsAssignableFrom(sortKeyType) || sortKeyType.IsAssignableFrom(x));
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Sort comparer {ComparerType} of column '{ColumnId}' does " +
        "not compare the {SortKeyType} keys its SortExpression produces. The comparer is skipped and the " +
        "column is sorted with the default comparison.")]
    private static partial void SortComparerNotApplicable(ILogger logger, string columnId, string comparerType,
        string sortKeyType);
}
