using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Moveable.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Moveable.Extensions;

public sealed class IServiceCollectionExtensionsTests
{
    [Fact]
    public async Task Assert_add_moveable()
    {
        // Arrange
        var services = new ServiceCollection()
            .MockServicesForMoveInteraction()
            .AddMoveable();

        await using var serviceProvider = services.BuildServiceProvider();

        // Act, Assert
        serviceProvider.AssertMoveableServices();
    }
}
