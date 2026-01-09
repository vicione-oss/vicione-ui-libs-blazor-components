using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Represents a <see cref="Grid{TGridItem}"/> column whose cells render a supplied template.
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
public sealed class TemplateColumn<TGridItem> : Microsoft.AspNetCore.Components.QuickGrid.TemplateColumn<TGridItem>
{
    [CascadingParameter]
    private new Grid<TGridItem> Grid { get; set; } = default!;

    /// <inheritdoc/>
    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        this.BeforeCellContent(builder, item, Grid);
        base.CellContent(builder, item);
        this.AfterCellContent(builder, item, Grid);
    }
}
