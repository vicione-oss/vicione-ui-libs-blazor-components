using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.TestingHelpers.Popup.Extensions;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Dialog.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>
/// </summary>
public static class IServiceProviderExtensions
{
    /// <summary>
    /// Asserts that services added via <see cref="Components.Dialog.Extensions.IServiceCollectionExtensions.AddDialog(IServiceCollection)"/>
    /// are registered in the specified <paramref name="serviceProvider"/>.
    /// </summary>
    public static void AssertDialogServices(this IServiceProvider serviceProvider)
        => serviceProvider.AssertPopupServices();
}
