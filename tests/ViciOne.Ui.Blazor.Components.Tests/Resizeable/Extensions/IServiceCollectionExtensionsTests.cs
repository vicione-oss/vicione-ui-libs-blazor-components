using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Resizeable.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Resizeable.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Resizeable.Extensions;

public sealed class IServiceCollectionExtensionsTests
{
    [Fact]
    public async Task Assert_add_resizeable()
    {
        // Arrange
        var services = new ServiceCollection()
            .MockServicesForResizeInteraction()
            .AddResizeable();

        await using var serviceProvider = services.BuildServiceProvider();

        // Act, Assert
        serviceProvider.AssertResizeableServices();
    }
}
