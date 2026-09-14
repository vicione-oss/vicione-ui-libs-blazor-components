using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Grid.Components.Shared;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;
using QuickGrid = Microsoft.AspNetCore.Components.QuickGrid;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Represents a <see cref="Grid{TGridItem}"/> column whose cells display a single value.
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
/// <typeparam name="TProp">The type of the value being displayed in the column's cells.</typeparam>
public sealed class PropertyColumn<TGridItem, TProp> : QuickGrid.PropertyColumn<TGridItem, TProp>
{
    private static readonly RenderFragment<QuickGrid.ColumnBase<TGridItem>> s_ownHeaderTemplate = column => builder =>
    {
        if (column is not PropertyColumn<TGridItem, TProp> propertyColumn)
            return;

        if (propertyColumn.Sortable ?? propertyColumn.IsSortableByDefault())
        {
            RenderFragments.SortableColumnHeader<TGridItem>()(column)(builder);
            return;
        }

        RenderFragments.NonSortableColumnHeader<TGridItem>()(column)(builder);
    };

    [CascadingParameter]
    private new Grid<TGridItem> Grid { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Class?.Length > 0 && Class != "property-column" && !Class.Contains(" property-column", StringComparison.Ordinal))
            Class += " property-column";
        else
            Class = "property-column";

        HeaderTemplate ??= s_ownHeaderTemplate;
    }

    /// <inheritdoc/>
    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        this.BeforeCellContent(builder, item, Grid);
        base.CellContent(builder, item);
        this.AfterCellContent(builder, item, Grid);
    }
}
