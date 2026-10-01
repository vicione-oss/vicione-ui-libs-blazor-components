namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// A filter that is not scoped to a column — an overarching criterion evaluated over the whole row, driven
/// from a control outside the column headers. Data only, no application logic.
/// </summary>
/// <remarks>
/// <para>Keyed by its CLR type rather than a column id, so two different global-filter types both stay
/// active and compose, while a second instance of the same type replaces the first.</para>
/// <para>A global filter is never dropped when a column disappears, because it names no column. The
/// column-scoped counterpart is <see cref="IColumnFilter"/>; implementing both is not meaningful, since the
/// two are keyed differently and <see cref="FilterState.WithGlobalFilter"/> rejects it.</para>
/// </remarks>
public interface IGlobalFilter : IFilter;
