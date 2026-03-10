using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Toolbar.Extensions;

namespace Shared.Pages.Toolbar.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddToolbarPage(this IServiceCollection services)
        => services.AddToolbar();
}
