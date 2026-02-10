using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.PointerCapture.Extensions;

public static class IServiceProviderExtensions
{
    public static void AssertSnapToGridPointerCaptureBehaviorService(this IServiceProvider serviceProvider)
    {
        // Act
        var services = serviceProvider.GetService<ISnapToGridPointerCaptureBehavior>();

        // Assert
        services.Should().BeOfType<SnapToGridPointerCaptureBehavior>();
    }
}
