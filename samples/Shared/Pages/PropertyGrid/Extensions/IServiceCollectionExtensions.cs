using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.PropertyGrid.Models;
using Shared.Pages.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

namespace Shared.Pages.PropertyGrid.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPropertyGridPage(this IServiceCollection services)
    {
        services.AddPropertyGrid<ExamplePropertyGridContext>()
            .WithPropertyDescriptorProvider<ExampleFooInstancePropertyDescriptorProvider>()
            .WithPropertyDescriptorProvider<ExampleBarInstancePropertyDescriptorProvider>()
            .WithPropertyValueEqualityComparer<string, ExampleStringPropertyValueEqualityComparer>();

        return services;
    }
}
