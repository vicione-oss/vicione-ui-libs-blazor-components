using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

/// <summary>
/// A filter that <see cref="SimpleTable{TItem}"/> applies itself, in-memory, over the whole item.
/// Adds an in-memory per-row predicate to the data-only <see cref="IFilter"/>:
/// <see cref="SimpleTableItemsProvider{TItem}"/> calls <see cref="Matches"/> for every
/// row. This baseline carries no column id, so a consumer can supply a global filter — a whole-item predicate
/// driven from a control outside the table — that composes with column filters. A column-scoped filter adds
/// the id through <see cref="ISimpleTableColumnFilter{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The item type of the parent <see cref="SimpleTable{TItem}"/>.</typeparam>
public interface ISimpleTableFilter<TItem> : IFilter
    where TItem : class
{
    /// <summary>
    /// Whether <paramref name="item"/> satisfies this filter.
    /// </summary>
    bool Matches(TItem item);
}
