namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// A filter scoped to one column, keyed by <see cref="ColumnId"/>. Data only, no application logic.
/// Table-agnostic: a consumer reads it and applies it however it queries data.
/// </summary>
/// <remarks>
/// A filter that also carries a per-row predicate does so through a separate, applying interface, keeping
/// predicate logic off this base. The scope-free counterpart is <see cref="IGlobalFilter"/>.
/// </remarks>
public interface IColumnFilter : IFilter
{
    /// <summary>
    /// The id of the column this filter applies to.
    /// </summary>
    string ColumnId { get; }
}
