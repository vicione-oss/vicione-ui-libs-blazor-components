using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Tables.Shared.Rows.ContextMenu.Models;
using ViciOne.Ui.Blazor.Components.ContextMenu.Extensions;

namespace Shared.Pages.Tables.SimpleTable.Rows.ContextMenu.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddRowContextMenuPage(this IServiceCollection services)
    {
        services.AddContextMenuCore();
        services.AddContextMenuRequest<ExampleRowContextMenuContext>();

        return services;
    }
}
