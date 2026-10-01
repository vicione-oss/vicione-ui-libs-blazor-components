using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Localization.Resources;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;

/// <summary>
/// A footer for <see cref="AdvancedTable{TItem}"/> that displays the number of items in the table and, where
/// one is supplied, the number of selected items.
/// </summary>
public sealed partial class TableFooter<TItem> : FooterBase<TItem>
    where TItem : class
{
    /// <summary>
    /// The number of items currently selected in the table. Left unset, no selection count is displayed.
    /// </summary>
    /// <remarks>
    /// <c>null</c> and <c>0</c> mean the same thing here — both leave the selection count out. The parameter is
    /// nullable so both counts of the footer present the same surface, not to tell the two apart.
    /// </remarks>
    [Parameter]
    public int? SelectedItemCount { get; set; }

    private string GetContentString()
    {
        var elements = CommonVocabulary.ElementPlural;

        if (EffectiveItemCount is 1)
            elements = CommonVocabulary.Element;

        if (SelectedItemCount is null or 0)
            return $"{EffectiveItemCount} {elements}";

        return $"{EffectiveItemCount} {elements} | {SelectedItemCount} {CommonVocabulary.Selected}";
    }
}
