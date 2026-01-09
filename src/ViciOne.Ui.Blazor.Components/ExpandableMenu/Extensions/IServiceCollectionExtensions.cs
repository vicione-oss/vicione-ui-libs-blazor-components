using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.ExpandableMenu.Services;
using ExpandableMenuComponent = ViciOne.Ui.Blazor.Components.ExpandableMenu.Components.ExpandableMenu;

namespace ViciOne.Ui.Blazor.Components.ExpandableMenu.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for <see cref="ExpandableMenuComponent">ExpandableMenu</see>
    /// </summary>
    public static IServiceCollection AddExpandableMenu(this IServiceCollection services)
        => services.AddScoped<ExpandableMenuService>();
}
