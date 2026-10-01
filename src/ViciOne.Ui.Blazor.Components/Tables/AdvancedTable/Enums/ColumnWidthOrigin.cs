namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Enums;

/// <summary>
/// Who pinned the live width a table holds for a column.
/// </summary>
internal enum ColumnWidthOrigin
{
    /// <summary>
    /// The column stays a flex column, so the next change in available space computes the width again.
    /// </summary>
    Resolved,

    /// <summary>
    /// The user pinned the width by dragging the column's resize handle; available space never moves it again.
    /// </summary>
    UserFixed
}
