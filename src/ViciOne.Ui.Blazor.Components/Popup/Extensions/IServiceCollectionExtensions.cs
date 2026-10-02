using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViciOne.Ui.Blazor.Components.FocusTrap.Extensions;
using ViciOne.Ui.Blazor.Components.Moveable.Extensions;
using ViciOne.Ui.Blazor.Components.Popup.Services;

namespace ViciOne.Ui.Blazor.Components.Popup.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for popup functionality
    /// </summary>
    public static IServiceCollection AddPopup(this IServiceCollection services)
    {
        services.AddFocusTrap();
        services.AddMoveable();

        services.TryAddScoped<IPopupRegistry, PopupRegistry>();

        return services;
    }
}
