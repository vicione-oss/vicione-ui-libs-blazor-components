using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace Shared.Pages.ToolTip.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTooltipPage(this IServiceCollection services)
        => services.AddTooltip();
}
