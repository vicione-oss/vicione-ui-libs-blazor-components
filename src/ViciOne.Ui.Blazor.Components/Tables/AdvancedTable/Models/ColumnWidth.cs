using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// A column's committed pixel width, as reported by JavaScript once a drag-resize has finished.
/// </summary>
[GenerateTypeScriptClass]
public sealed record ColumnWidth
{
    /// <summary>
    /// Identifies the column the width belongs to.
    /// </summary>
    public required string ColumnId { get; init; }

    /// <summary>
    /// The width in pixels.
    /// </summary>
    public required double Value { get; init; }
}
