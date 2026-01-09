using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection MockServicesForMoveInteraction(this IServiceCollection services)
        => services.AddScoped(_ => Substitute.For<ILogger<MoveInteraction>>())
            .AddScoped(_ => Substitute.For<IJSRuntime>());
}
