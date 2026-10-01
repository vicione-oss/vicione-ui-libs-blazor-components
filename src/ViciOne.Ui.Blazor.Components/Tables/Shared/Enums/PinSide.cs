namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

/// <summary>
/// Specifies whether a column of the <see cref="AdvancedTable.Components.AdvancedTable{TItem}"/> is pinned to an edge so it stays
/// visible during horizontal scroll.
/// </summary>
public enum PinSide
{
    /// <summary>
    /// The column is not pinned and scrolls horizontally with the table body.
    /// </summary>
    None = 0,

    /// <summary>
    /// The column is pinned to the left edge.
    /// </summary>
    Left = 1,

    /// <summary>
    /// The column is pinned to the right edge.
    /// </summary>
    Right = 2
}
