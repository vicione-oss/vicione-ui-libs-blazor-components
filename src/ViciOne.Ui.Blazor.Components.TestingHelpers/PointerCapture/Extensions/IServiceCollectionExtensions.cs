using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.PointerCapture.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds substitute services for <see cref="IJSRuntime"/> and <see cref="ILogger{SnapToGridPointerCaptureBehavior}"/> required for testing <see cref="SnapToGridPointerCaptureBehavior"/>
    /// </summary>
    public static IServiceCollection MockServicesForSnapToGridPointerCaptureBehavior(this IServiceCollection services)
    {
        services.TryAddScoped(_ => Substitute.For<ILogger<SnapToGridPointerCaptureBehavior>>());
        services.TryAddScoped(_ => Substitute.For<IJSRuntime>());

        return services;
    }
}
