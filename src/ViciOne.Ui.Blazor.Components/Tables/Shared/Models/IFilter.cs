namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// A single filter criterion held by a <see cref="FilterState"/>. Data only, no application logic, and
/// non-generic so it stays usable in the table-agnostic <see cref="FilterState"/> value object and by a
/// consumer that filters its own data source.
/// </summary>
/// <remarks>
/// Every filter is either scoped to a column (<see cref="IColumnFilter"/>) or global
/// (<see cref="IGlobalFilter"/>). A filter that also carries a per-row predicate adds that through a
/// separate, generic interface, keeping predicate logic off this base.
/// </remarks>
public interface IFilter;
