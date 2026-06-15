using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Draggable.Services;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Draggable.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that services added via <see cref="Blazor.Components.Draggable.Extensions.IServiceCollectionExtensions.AddDraggable(IServiceCollection)"/>
    /// are registered in the specified <paramref name="serviceProvider"/>.
    /// </summary>
    public static void AssertDraggableServices(this IServiceProvider serviceProvider)
    {
        // Act
        var dragInteraction = serviceProvider.GetService<IDragInteraction>();

        // Assert
        dragInteraction.Should().BeOfType<DragInteraction>();
    }
}
