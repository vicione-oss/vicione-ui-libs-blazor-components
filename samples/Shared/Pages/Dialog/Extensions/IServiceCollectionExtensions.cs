using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Dialog.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

namespace Shared.Pages.Dialog.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDialogPage(this IServiceCollection services)
    {
        services.AddNullableIntSpinEdit();

        services.AddDialog();

        return services;
    }
}
