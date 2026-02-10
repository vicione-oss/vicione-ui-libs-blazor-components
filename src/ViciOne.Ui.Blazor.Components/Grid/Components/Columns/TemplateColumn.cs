using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Grid.Components.Shared;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;
using ViciOne.Ui.Blazor.Components.Grid.Models;
using QuickGrid = Microsoft.AspNetCore.Components.QuickGrid;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Represents a <see cref="Grid{TGridItem}"/> column whose cells render a supplied template.
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
public sealed class TemplateColumn<TGridItem> : QuickGrid.TemplateColumn<TGridItem>
{
    private static readonly RenderFragment<QuickGrid.ColumnBase<TGridItem>> s_ownHeaderTemplate = column => builder =>
    {
        if (column is not TemplateColumn<TGridItem> templateColumn)
            return;

        if (templateColumn.Sortable ?? templateColumn.IsSortableByDefault())
        {
            RenderFragments.SortableColumnHeader<TGridItem>()(column)(builder);
            return;
        }

        RenderFragments.NonSortableColumnHeader<TGridItem>()(column)(builder);
    };

    [CascadingParameter]
    private new Grid<TGridItem> Grid { get; set; } = default!;

    /// <inheritdoc cref="QuickGrid.TemplateColumn{TGridItem}.SortBy"/>
    [Parameter] public GridSort<TGridItem>? Sort { get; set; }

    /// <summary>
    /// Make parameter inaccessable in razor files to enforce use of <see cref="Sort"/> instead,
    /// which is converted to the base class <see cref="QuickGrid.ColumnBase{TGridItem}.SortBy"/> in <see cref="OnParametersSet"/>."/>
    /// </summary>
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable RCS1213 // Remove unused member declaration
#pragma warning disable RCS1170 // Use read-only auto-implemented property
    private new QuickGrid.GridSort<TGridItem>? SortBy { get; set; }
#pragma warning restore RCS1170 // Use read-only auto-implemented property
#pragma warning restore RCS1213 // Remove unused member declaration
#pragma warning restore IDE0051 // Remove unused private members

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.SortBy = Sort?.ToQuickGridSort();

        base.OnParametersSet();

        if (Class?.Length > 0 && Class != "template-column" && !Class.Contains(" template-column", StringComparison.Ordinal))
            Class += " template-column";
        else
            Class = "template-column";

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
