using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;

namespace Shared.Pages.Breadcrumb.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBreadcrumbPage(this IServiceCollection services)
        => services.AddBreadcrumb();
}
