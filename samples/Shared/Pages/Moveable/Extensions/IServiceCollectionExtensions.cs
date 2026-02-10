using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Moveable.Extensions;
using ViciOne.Ui.Blazor.Components.PointerCapture.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

namespace Shared.Pages.Moveable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMoveablePage(this IServiceCollection services)
    {
        services.AddIntSpinEdit();

        services.AddMoveable();

        services.AddSnapToGridPointerCaptureBehavior();

        return services;
    }
}
