using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Services;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;

/// <summary>
/// A column filter that <see cref="SimpleTable{TItem}"/> applies itself, in-memory. Combines the
/// in-memory per-row predicate of <see cref="ISimpleTableFilter{TItem}"/> with the column id of
/// <see cref="IColumnFilter"/>, so <see cref="SimpleTableItemsProvider{TItem}"/>
/// applies it only while its column is registered, with no manual column mapping required.
/// </summary>
/// <typeparam name="TItem">The item type of the parent <see cref="SimpleTable{TItem}"/>.</typeparam>
public interface ISimpleTableColumnFilter<TItem> : ISimpleTableFilter<TItem>, IColumnFilter
    where TItem : class;
