using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.AdvancedTable.Models;

/// <summary>
/// Matches rows whose value lies between two inclusive bounds.
/// </summary>
/// <remarks>
/// Data only, like every AdvancedTable filter: applying it is the items provider's job. See
/// <c>ExampleTableItemsProvider</c> for the matching half.
/// </remarks>
/// <param name="ColumnId">The id of the column this filter narrows.</param>
/// <param name="From">Inclusive lower bound, or <see langword="null"/> to leave that side unbounded.</param>
/// <param name="To">Inclusive upper bound, or <see langword="null"/> to leave that side unbounded.</param>
public sealed record IntRangeColumnFilter(string ColumnId, int? From, int? To) : IColumnFilter;
