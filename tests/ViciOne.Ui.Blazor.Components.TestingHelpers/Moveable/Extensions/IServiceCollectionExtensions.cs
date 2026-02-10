using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection MockServicesForMoveInteraction(this IServiceCollection services)
    {
        services.TryAddScoped(_ => Substitute.For<ILogger<MoveInteraction>>());
        services.TryAddScoped(_ => Substitute.For<IJSRuntime>());

        return services;
    }
}
