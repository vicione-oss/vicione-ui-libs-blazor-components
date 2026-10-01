using Shared.Pages.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace Shared.Pages.Tables.AdvancedTable.Components;

public sealed partial class StartsWithColumnFilterEditor
{
    private string _text = string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Context.ColumnFilter is StartsWithColumnFilter startsWithFilter)
            _text = startsWithFilter.Value;
    }

    protected override IColumnFilter? BuildFilter()
    {
        if (string.IsNullOrEmpty(_text))
            return null;

        return new StartsWithColumnFilter(Context.ColumnId, _text);
    }

    // Bound to TextChanging rather than @bind-Text: it fires per keystroke, so the text is current when Apply is
    // clicked instead of only after the input commits on blur.
    private void SearchBoxTextChanging(string? text)
        => _text = text ?? string.Empty;
}
