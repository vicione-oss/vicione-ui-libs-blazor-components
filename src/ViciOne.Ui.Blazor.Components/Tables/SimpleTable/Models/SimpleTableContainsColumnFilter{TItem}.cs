using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

/// <summary>
/// Premade, case-insensitive "contains" filter for a <see cref="SimpleTable{TItem}"/> column.
/// Projects each row to a string via <see cref="ValueSelector"/> and matches in-memory; a <see langword="null"/>
/// projection never matches. Public so it can be seeded programmatically.
/// </summary>
/// <remarks>
/// Carrying <see cref="ValueSelector"/> makes this filter behavior as well as data: equality includes the
/// delegate, so two instances built from separately written lambdas are never equal, and the value cannot be
/// serialized or restored from storage.
/// </remarks>
/// <typeparam name="TItem">The item type of the parent <see cref="SimpleTable{TItem}"/>.</typeparam>
/// <param name="ColumnId">The id of the column this filter applies to.</param>
/// <param name="Value">The text matched against the projected value.</param>
/// <param name="ValueSelector">
/// Projects a row to the string compared against <paramref name="Value"/>. Returning <see cref="string"/> lets the
/// consumer stringify any value type (<c>x => x.Age.ToString()</c>) and match the same text the cell renders.
/// </param>
public sealed record SimpleTableContainsColumnFilter<TItem>(
    string ColumnId,
    string Value,
    Func<TItem, string?> ValueSelector) : ISimpleTableColumnFilter<TItem>
    where TItem : class
{
    /// <inheritdoc/>
    public bool Matches(TItem item)
    {
        var value = ValueSelector(item);

        return value?.Contains(Value, StringComparison.OrdinalIgnoreCase) == true;
    }
}
