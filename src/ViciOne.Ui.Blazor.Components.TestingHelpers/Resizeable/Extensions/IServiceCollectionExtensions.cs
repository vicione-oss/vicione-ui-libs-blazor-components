using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds substitute services for <see cref="IJSRuntime"/> and <see cref="ILogger{ResizeInteraction}"/> required for testing <see cref="ResizeInteraction"/>
    /// </summary>
    public static IServiceCollection MockServicesForResizeInteraction(this IServiceCollection services)
    {
        services.TryAddScoped(_ => Substitute.For<ILogger<ResizeInteraction>>());
        services.TryAddScoped(_ => Substitute.For<IJSRuntime>());

        return services;
    }
}
