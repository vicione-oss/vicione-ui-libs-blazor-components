using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PointerCapture.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.PointerCapture.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PointerCapture.Extensions;

public sealed class IServiceCollectionExtensionsTests
{
    [Fact]
    public async Task Assert_add_snap_to_grid_pointer_capture_behavior()
    {
        // Arrange
        var services = new ServiceCollection()
            .MockServicesForSnapToGridPointerCaptureBehavior()
            .AddSnapToGridPointerCaptureBehavior();

        await using var serviceProvider = services.BuildServiceProvider();

        // Act, Assert
        serviceProvider.AssertSnapToGridPointerCaptureBehaviorService();
    }
}
