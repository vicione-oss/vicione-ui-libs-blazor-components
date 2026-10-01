namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// A column whose width the user can change by dragging the right edge of its header.
/// </summary>
internal interface IResizeableColumn
{
    /// <summary>
    /// <see langword="true"/> when the column can be resized, otherwise <see langword="false"/>.
    /// </summary>
    bool Resizeable { get; }

    /// <summary>
    /// Minimum width in pixels the column must have. The width can not be below this value.
    /// </summary>
    int MinimumWidth { get; }
}
