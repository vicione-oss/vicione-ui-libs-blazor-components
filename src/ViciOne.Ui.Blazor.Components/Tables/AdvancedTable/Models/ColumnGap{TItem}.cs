using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// A position a dragged column can be dropped into: between two adjacent columns of the same pin side, or at
/// either end of them.
/// </summary>
/// <param name="Column">The column the gap is anchored to, so the gap still resolves after the dragged column
/// has been removed from the column list.</param>
/// <param name="After">
/// <para><see langword="false"/>: the gap sits in front of <paramref name="Column"/>.</para>
/// <para><see langword="true"/>: the gap sits behind <paramref name="Column"/>, which only holds behind the
/// last column of a pin side, where no following column could anchor it.</para>
/// </param>
internal readonly record struct ColumnGap<TItem>(IAdvancedTableColumn<TItem> Column, bool After)
    where TItem : class;
