namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Defines the sorting direction for a specific column identified by its unique identifier.
/// </summary>
/// <param name="ColumnId">The unique identifier of the column to be sorted.</param>
/// <param name="Ascending"><see langword="true"/> to sort in ascending order; <see langword="false"/> for descending.</param>
public sealed record ColumnSorting(string ColumnId, bool Ascending);
