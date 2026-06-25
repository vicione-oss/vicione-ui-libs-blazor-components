using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizeable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that services added via <see cref="Components.Resizeable.Extensions.IServiceCollectionExtensions.AddResizeable(IServiceCollection)"/>
    /// are registered in the specified <paramref name="serviceProvider"/>.
    /// </summary>
    public static void AssertResizeableServices(this IServiceProvider serviceProvider)
    {
        // Act
        var resizeInteraction = serviceProvider.GetService<IResizeInteraction>();

        // Assert
        resizeInteraction.Should().BeOfType<ResizeInteraction>();
    }
}
