namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// An abstract base class for columns in a <see cref="Grid{TGridItem}"/>.
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
[Obsolete(Constants.ObsoleteMessage)]
public abstract class ColumnBase<TGridItem> : Microsoft.AspNetCore.Components.QuickGrid.ColumnBase<TGridItem>;
