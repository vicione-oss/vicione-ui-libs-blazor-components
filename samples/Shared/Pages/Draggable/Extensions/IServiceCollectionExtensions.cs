using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Pages.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Components;
using ViciOne.Ui.Blazor.Components.Draggable.Extensions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.PointerCapture.Extensions;
using ViciOne.Ui.Blazor.Components.SpinEdit.Extensions;

namespace Shared.Pages.Draggable.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDraggablePage(this IServiceCollection services)
    {
        services.AddDraggable();
        services.AddIntSpinEdit();
        services.AddSnapToGridPointerCaptureBehavior();

        services.TryAddScoped<SampleDropHandler>();
        services.TryAddScoped<IDropPolicy<IDropzone>, SampleDropPolicy>();
        services.TryAddScoped<IDropHandler<IDropzone>>(sp => sp.GetRequiredService<SampleDropHandler>());

        return services;
    }
}
