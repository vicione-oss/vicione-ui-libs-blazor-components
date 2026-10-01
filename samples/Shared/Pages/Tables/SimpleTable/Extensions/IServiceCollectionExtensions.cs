using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.Tables.SimpleTable.Rows.ContextMenu.Extensions;
using Shared.Pages.Tables.SimpleTable.Rows.DragAndDrop.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;

namespace Shared.Pages.Tables.SimpleTable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleTablePages(this IServiceCollection services)
    {
        services.AddSimpleTable();

        services.AddIntRangeColumnFilterEditor();

        services.AddRowContextMenuPage();

        services.AddRowsDragAndDropPage();

        return services;
    }

    private static IServiceCollection AddIntRangeColumnFilterEditor(this IServiceCollection services)
        => services.AddNullableIntSpinEdit();
}
