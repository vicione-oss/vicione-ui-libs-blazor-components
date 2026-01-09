using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;

namespace Shared.Pages.Grid.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddGridPage(this IServiceCollection services)
        => services.AddGridItemSelectColumn()
            .AddGridItemSelection<Guid>();
}
