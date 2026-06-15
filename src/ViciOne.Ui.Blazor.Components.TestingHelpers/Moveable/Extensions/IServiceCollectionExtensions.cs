using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds substitute services for <see cref="IJSRuntime"/> and <see cref="ILogger{MoveInteraction}"/>
    /// required for testing the implementation of <see cref="IMoveInteraction"/>.
    /// </summary>
    public static IServiceCollection MockServicesForMoveInteraction(this IServiceCollection services)
    {
        services.TryAddScoped(_ => Substitute.For<ILogger<MoveInteraction>>());
        services.TryAddScoped(_ => Substitute.For<IJSRuntime>());

        return services;
    }
}
