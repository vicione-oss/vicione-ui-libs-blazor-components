using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Grid.Extensions;

namespace Shared.Pages.Grid.Extensions;

public static class IServiceCollectionExtensions
{
    // the Grid page keeps demonstrating the obsolete Grid until it is removed
#pragma warning disable CS0618
    public static IServiceCollection AddGridPage(this IServiceCollection services)
        => services.AddGridItemSelectColumn()
            .AddGridItemSelection<Guid>();
#pragma warning restore CS0618
}
