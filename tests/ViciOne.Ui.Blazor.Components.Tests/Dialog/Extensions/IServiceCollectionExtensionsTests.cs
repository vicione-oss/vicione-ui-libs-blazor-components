using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Dialog.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Dialog.Extensions;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Dialog.Extensions;

public sealed class IServiceCollectionExtensionsTests
{
    [Fact]
    public async Task Assert_add_dialog()
    {
        // Arrange
        var services = new ServiceCollection()
            .MockServicesForMoveInteraction()
            .AddDialog();

        await using var serviceProvider = services.BuildServiceProvider();

        // Act, Assert
        serviceProvider.AssertDialogServices();
    }
}
