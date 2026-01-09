using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.CheckBox.Extensions;

namespace Shared.Pages.CheckBox.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCheckBoxPage(this IServiceCollection services)
        => services.AddCheckBox();
}
