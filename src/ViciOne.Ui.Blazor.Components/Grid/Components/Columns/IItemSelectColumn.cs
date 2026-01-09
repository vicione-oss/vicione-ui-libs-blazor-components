namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Contract for a column that can determine selection state for grid items.
/// </summary>
/// <typeparam name="TGridItem">The item/row type handled by the grid column.</typeparam>
internal interface IItemSelectColumn<TGridItem>
{
    /// <summary>
    /// Determines whether the specified grid item is currently selected.
    /// </summary>
    /// <param name="item">The <typeparamref name="TGridItem"/> to evaluate. Implementations should define how null values are handled; typically a null item is not selected.</param>
    /// <returns><c>true</c> when the given <paramref name="item"/> is considered selected; otherwise <c>false</c>.</returns>
    bool IsSelected(TGridItem item);
}
