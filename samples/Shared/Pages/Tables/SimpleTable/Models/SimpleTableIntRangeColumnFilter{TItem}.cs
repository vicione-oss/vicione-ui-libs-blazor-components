using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace Shared.Pages.Tables.SimpleTable.Models;

/// <summary>
/// Matches rows whose projected value lies between two inclusive bounds.
/// </summary>
/// <remarks>
/// Carries <c>Matches</c>, so <c>SimpleTable</c>'s built-in provider applies it in-memory with nothing else wired
/// up — unlike the AdvancedTable side, where the consumer's items provider does the matching.
/// </remarks>
/// <param name="ColumnId">The id of the column this filter narrows.</param>
/// <param name="From">Inclusive lower bound, or <see langword="null"/> to leave that side unbounded.</param>
/// <param name="To">Inclusive upper bound, or <see langword="null"/> to leave that side unbounded.</param>
/// <param name="ValueSelector">
/// Projects a row to the value compared against the bounds. Projects the raw number rather than display text,
/// because a range compares the underlying value. A <see langword="null"/> projection never matches.
/// </param>
public sealed record SimpleTableIntRangeColumnFilter<TItem>(string ColumnId, int? From, int? To,
    Func<TItem, int?> ValueSelector)
        : ISimpleTableColumnFilter<TItem>
            where TItem : class
{
    public bool Matches(TItem item)
    {
        if (ValueSelector(item) is not { } value)
            return false;

        return (From is null || value >= From.Value)
            && (To is null || value <= To.Value);
    }
}
