using Microsoft.AspNetCore.Components;
using Shared.Pages.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.Components;

public sealed partial class SimpleTableStartsWithColumnFilterEditor<TItem>
    where TItem : class
{
    private string _text = string.Empty;

    [Parameter, EditorRequired]
    public required Func<TItem, string?> ValueSelector { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Context.ColumnFilter is SimpleTableStartsWithColumnFilter<TItem> startsWithFilter)
            _text = startsWithFilter.Value;
    }

    protected override IColumnFilter? BuildFilter()
    {
        if (string.IsNullOrEmpty(_text))
            return null;

        return new SimpleTableStartsWithColumnFilter<TItem>(Context.ColumnId, _text, ValueSelector);
    }

    // Bound to TextChanging rather than @bind-Text: it fires per keystroke, so the text is current when Apply is
    // clicked instead of only after the input commits on blur.
    private void SearchBoxTextChanging(string? text)
        => _text = text ?? string.Empty;
}
