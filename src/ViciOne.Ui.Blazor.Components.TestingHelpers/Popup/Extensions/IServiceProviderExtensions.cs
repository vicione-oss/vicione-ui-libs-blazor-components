using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Popup.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Moveable.Extensions;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that services added via <see cref="Components.Popup.Extensions.IServiceCollectionExtensions.AddPopup(IServiceCollection)"/>
    /// are registered in the specified <paramref name="serviceProvider"/>.
    /// </summary>
    public static void AssertPopupServices(this IServiceProvider serviceProvider)
    {
        // Act
        var popupRegistry = serviceProvider.GetService<IPopupRegistry>();

        // Assert
        popupRegistry.Should().BeOfType<PopupRegistry>();

        serviceProvider.AssertMoveableServices();
    }
}
