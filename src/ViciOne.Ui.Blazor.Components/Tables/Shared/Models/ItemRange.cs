namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Contains which item range should be loaded and displayed for a table.
/// </summary>
/// <param name="Skip">How many of the items are to be skipped (range starts after the skipped items).</param>
/// <param name="Take">How many of the items are to be shown (range ends with the last item to be shown).</param>
public sealed record ItemRange(int Skip, int Take);
