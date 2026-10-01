namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

/// <summary>
/// A column with sorting capabilities.
/// </summary>
internal interface ISortableColumn
{
    /// <summary>
    /// <see langword="true"/> when the column unlocks its sorting capabilities, otherwise <see langword="false"/>.
    /// </summary>
    bool Sortable { get; }
}
