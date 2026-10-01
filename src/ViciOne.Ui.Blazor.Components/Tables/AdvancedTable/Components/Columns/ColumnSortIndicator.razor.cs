using Microsoft.AspNetCore.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// The arrow a sorted column's header carries, pointing the way that column's values are ordered.
/// </summary>
public sealed partial class ColumnSortIndicator : ComponentBase
{
    /// <summary>
    /// The direction the column is sorted in: ascending points up, descending points down.
    /// </summary>
    [Parameter, EditorRequired]
    public required bool Ascending { get; set; }

    private MonochromeIconName IconName
    {
        get
        {
            if (Ascending)
                return MonochromeIconName.MoveUp;

            return MonochromeIconName.MoveDown;
        }
    }
}
