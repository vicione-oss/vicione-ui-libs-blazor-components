using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizeable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Sidebar.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for using the sidebar.
    /// </summary>
    public static IServiceCollection AddSidebar(this IServiceCollection services)
        => services.AddResizeable();
}
