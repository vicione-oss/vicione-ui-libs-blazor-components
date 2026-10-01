namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

/// <summary>
/// Specifies the data loading strategy for the <see cref="AdvancedTable.Components.AdvancedTable{TItem}"/>.
/// </summary>
public enum TableLoadingMode
{
    /// <summary>
    /// Loads all items in the collection at once.
    /// Best for small datasets where the entire list is held in memory.
    /// </summary>
    All = 0,

    /// <summary>
    /// Uses UI virtualization to load only the items currently visible in the viewport.
    /// Ideal for large datasets to maintain high performance and low memory footprint.
    /// </summary>
    Virtualize = 1
}
