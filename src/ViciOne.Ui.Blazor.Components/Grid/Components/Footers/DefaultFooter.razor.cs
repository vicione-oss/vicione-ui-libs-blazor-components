using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Localization.Resources;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Footers;

/// <summary>
///  Represents a <see cref="Grid{TGridItem}"/> footer, which contains the total number of elements.
/// </summary>
public sealed partial class DefaultFooter
{
    [CascadingParameter]
    private int ItemCount { get; set; } = default!;

    private string GetContentString()
    {
        var elements = CommonVocabulary.ElementPlural;

        if (ItemCount is 1)
            elements = CommonVocabulary.Element;

        return $"{ItemCount} {elements}";
    }
}
