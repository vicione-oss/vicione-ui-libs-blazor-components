using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.PointerCapture.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that service provider has registered <see cref="ISnapToGridPointerCaptureBehavior"/>.
    /// </summary>
    public static void AssertSnapToGridPointerCaptureBehaviorService(this IServiceProvider serviceProvider)
    {
        // Act
        var services = serviceProvider.GetService<ISnapToGridPointerCaptureBehavior>();

        // Assert
        services.Should().BeOfType<SnapToGridPointerCaptureBehavior>();
    }
}
