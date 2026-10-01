using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.CheckBox.Extensions;
using ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;
using ViciOne.Ui.Blazor.Components.Draggable.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="AdvancedTable{TItem}"/>.
    /// </summary>
    public static IServiceCollection AddAdvancedTable(this IServiceCollection services)
    {
        services.AddDraggable();
        services.AddCheckBox();

        // The table reads IContextMenuSettings.UseCustomMenu to decide whether to suppress the browser's
        // native menu on a row right-click.
        services.AddContextMenuCore();

        return services;
    }
}
