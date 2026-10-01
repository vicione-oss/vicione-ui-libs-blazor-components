using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Pages.Tables.Shared.Models;
using Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Services;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;

namespace Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddRowsDragAndDropPage(this IServiceCollection services)
    {
        services.TryAddScoped<VisibleSelectionStore>();
        services.TryAddScoped<ITableDragPayloadProvider<ExampleTableItem>, VisibleSelectionDragPayloadProvider>();

        return services;
    }
}
