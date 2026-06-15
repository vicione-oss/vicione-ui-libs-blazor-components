using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds substitute services for <see cref="IJSRuntime"/> and <see cref="ILogger{DragInteraction}"/>
    /// required for testing the implementation of <see cref="IDragInteraction"/>.
    /// </summary>
    public static IServiceCollection MockServicesForDragInteraction(this IServiceCollection services)
    {
        services.TryAddScoped(_ => Substitute.For<ILogger<DragInteraction>>());
        services.TryAddScoped(_ => Substitute.For<IJSRuntime>());

        return services;
    }
}
