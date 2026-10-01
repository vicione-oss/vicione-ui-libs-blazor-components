using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// A live column width the table holds, together with who pinned it.
/// </summary>
/// <remarks>
/// The origin cannot be inferred from the width being present: laying the columns out writes a width for
/// every flex column, and so does the drag gesture for the columns it has to pin before it can move one.
/// Without the origin the first drag would make every column of the table user-fixed.
/// </remarks>
/// <param name="Value">The width in pixels.</param>
/// <param name="Origin">Who pinned the width.</param>
internal sealed record ColumnWidthOverride(double Value, ColumnWidthOrigin Origin);
