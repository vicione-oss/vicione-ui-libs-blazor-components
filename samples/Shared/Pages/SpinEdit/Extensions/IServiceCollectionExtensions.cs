using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

namespace Shared.Pages.SpinEdit.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSpinEditPage(this IServiceCollection services)
        => services.AddIntSpinEdit()
            .AddNullableIntSpinEdit()
            .AddEnumSpinEdit<ButtonSize>();
}
