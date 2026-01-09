using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Grid.Components;

namespace ViciOne.Ui.Blazor.Components.Grid.Extensions;

internal static class ColumnBaseExtensions
{
    public static void BeforeCellContent<TGridItem>(this ColumnBase<TGridItem> _, RenderTreeBuilder builder, TGridItem item, Grid<TGridItem> grid)
    {
        if (IsGridItemSelected(item, grid))
        {
            builder.OpenRegion(0);
            builder.OpenElement(1, "div");
            builder.AddAttribute(2, "class", "selected");
        }
    }

    public static void AfterCellContent<TGridItem>(this ColumnBase<TGridItem> _, RenderTreeBuilder builder, TGridItem item, Grid<TGridItem> grid)
    {
        if (IsGridItemSelected(item, grid))
        {
            builder.CloseElement();
            builder.CloseRegion();
        }
    }

    private static bool IsGridItemSelected<TGridItem>(TGridItem item, Grid<TGridItem> grid)
    {
        if (grid?.ItemSelectColumn is not null)
            return grid.ItemSelectColumn.IsSelected(item);
        else
            return false;
    }
}
