namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// A column header asking to be sorted by.
/// </summary>
/// <param name="Ascending">The direction asked for, already toggled against the column's current one.</param>
/// <param name="Additive">
/// <para><see langword="false"/>: sort by this column alone, replacing the current sorting.</para>
/// <para><see langword="true"/>: add this column to the sorting already applied, which is what
/// <kbd>Shift</kbd> asks for.</para>
/// </param>
public readonly record struct SortRequest(bool Ascending, bool Additive);
