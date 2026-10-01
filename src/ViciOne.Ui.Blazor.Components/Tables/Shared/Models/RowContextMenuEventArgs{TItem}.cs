using Microsoft.AspNetCore.Components.Web;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Provides data for a row right-click: the row's data item and the mouse event that triggered it.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
/// <param name="item">The data item of the row that was right-clicked.</param>
/// <param name="mouseEventArgs">The mouse event raised by the right-click.</param>
public sealed class RowContextMenuEventArgs<TItem>(TItem item, MouseEventArgs mouseEventArgs) : EventArgs
    where TItem : class
{
    /// <summary>
    /// The data item of the row that was right-clicked.
    /// </summary>
    public TItem Item { get; } = item;

    /// <summary>
    /// The mouse event raised by the right-click, carrying the cursor position.
    /// </summary>
    public MouseEventArgs MouseEventArgs { get; } = mouseEventArgs;
}
