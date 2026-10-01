using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Filters;

/// <summary>
/// UI editor for the premade <see cref="SimpleTableContainsColumnFilter{TItem}"/>, placed in a column's
/// <c>FilterEditor</c> slot. Offers a text input and commits a case-insensitive contains filter
/// (or clears it when empty). Matching is always in-memory — see <see cref="SimpleTableContainsColumnFilter{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The item type of the parent <see cref="SimpleTable{TItem}"/>.</typeparam>
public sealed partial class SimpleTableContainsColumnFilterEditor<TItem>
    where TItem : class
{
    private string _text = string.Empty;

    /// <summary>
    /// Projects an item to the string compared against the filter value. Returns <see cref="string"/> so the
    /// consumer stringifies any value type (<c>x => x.Age.ToString()</c>) and matches the text the cell renders.
    /// </summary>
    [Parameter, EditorRequired]
    public required Func<TItem, string?> ValueSelector { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Context.ColumnFilter is SimpleTableContainsColumnFilter<TItem> containsFilter)
            _text = containsFilter.Value;
    }

    /// <inheritdoc/>
    protected override IColumnFilter? BuildFilter()
    {
        if (string.IsNullOrEmpty(_text))
            return null;

        return new SimpleTableContainsColumnFilter<TItem>(Context.ColumnId, _text, ValueSelector);
    }

    // Bound to TextChanging rather than @bind-Text: it fires per keystroke, so the text is current when Apply is
    // clicked instead of only after the input commits on blur.
    private void SearchBoxTextChanging(string? text)
        => _text = text ?? string.Empty;
}
