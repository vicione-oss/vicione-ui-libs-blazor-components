using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Extensions;

public static class IServiceProviderExtensions
{
    public static void AssertMoveableServices(this IServiceProvider serviceProvider)
    {
        // Act
        var moveInteraction = serviceProvider.GetService<IMoveInteraction>();

        // Assert
        moveInteraction.Should().BeOfType<MoveInteraction>();
    }
}
