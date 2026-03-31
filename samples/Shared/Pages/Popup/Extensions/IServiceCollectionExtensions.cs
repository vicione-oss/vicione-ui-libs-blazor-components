using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

namespace Shared.Pages.Popup.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPopupPage(this IServiceCollection services)
    {
        services.AddNullableIntSpinEdit();

        services.AddPopup();

        return services;
    }
}
