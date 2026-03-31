using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Popup.Extensions;

namespace ViciOne.Ui.Blazor.Components.Dialog.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for dialog functionality
    /// </summary>
    public static IServiceCollection AddDialog(this IServiceCollection services)
    {
        services.AddPopup();

        return services;
    }
}
