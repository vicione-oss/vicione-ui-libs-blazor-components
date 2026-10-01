namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

/// <summary>
/// Provides context to the <see cref="Tables.AdvancedTable.Components.AdvancedTable{TItem}.ItemSelectionAllowed"/> predicate,
/// describing which item is being evaluated for selection and what is currently selected.
/// </summary>
/// <typeparam name="TItem">The type of data represented by each row in the table.</typeparam>
public sealed record SelectionRequest<TItem>(
    TItem Item,
    IReadOnlyList<TItem> CurrentSelection)
    where TItem : class;
