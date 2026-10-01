using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Tables.Shared.Rows.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Extensions;

namespace Shared.Pages.Tables.AdvancedTable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAdvancedTablePages(this IServiceCollection services)
    {
        services.AddAdvancedTable();

        services.AddIntRangeColumnFilterEditor();

        services.AddRowContextMenuPage();

        return services;
    }

    private static IServiceCollection AddRowContextMenuPage(this IServiceCollection services)
    {
        services.AddContextMenuCore();
        services.AddContextMenuRequest<ExampleRowContextMenuContext>();

        return services;
    }

    private static IServiceCollection AddIntRangeColumnFilterEditor(this IServiceCollection services)
        => services.AddNullableIntSpinEdit();
}
