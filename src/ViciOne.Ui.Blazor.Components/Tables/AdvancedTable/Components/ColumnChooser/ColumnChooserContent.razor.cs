using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;

/// <summary>
/// The chooser-eligible columns of a table, one checkbox row per column. Rendered by
/// <see cref="AdvancedTable{TItem}"/> into whichever <see cref="ColumnChooserToggle"/> has the matching
/// <see cref="ColumnChooserToggle.Id"/>, which owns the trigger button and popup chrome around this list.
/// </summary>
public sealed partial class ColumnChooserContent<TItem> : ComponentBase
    where TItem : class
{
    /// <summary>
    /// The table whose columns this chooser controls, cascaded down by the
    /// <see cref="AdvancedTable{TItem}"/> that renders this chooser.
    /// </summary>
    [CascadingParameter]
    internal IAdvancedTable<TItem> Table { get; set; } = default!;

    // Computed per access: ShowInColumnChooser is a mutable parameter, so a cache invalidated only on
    // register/unregister would go stale when a consumer flips the flag at runtime.
    // A column carries no state once the table has unregistered it; the markup skips those rather than
    // dereference a dropped state and take the circuit down mid-render.
    private IEnumerable<IAdvancedTableColumn<TItem>> ChooserColumns
        => Table.Columns.Where(c => c.ShowInColumnChooser);
}
