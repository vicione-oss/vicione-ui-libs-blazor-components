using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Moveable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that service provider has registered <see cref="IMoveInteraction"/>.
    /// </summary>
    public static void AssertMoveableServices(this IServiceProvider serviceProvider)
    {
        // Act
        var moveInteraction = serviceProvider.GetService<IMoveInteraction>();

        // Assert
        moveInteraction.Should().BeOfType<MoveInteraction>();
    }
}
