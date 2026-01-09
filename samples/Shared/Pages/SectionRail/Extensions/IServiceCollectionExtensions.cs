using Microsoft.Extensions.DependencyInjection;
using Shared.Pages.SectionRail.Enums;
using ViciOne.Ui.Blazor.Components.SectionRail.Extensions;

namespace Shared.Pages.SectionRail.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSectionRailPage(this IServiceCollection services)
        => services.AddSectionRail<SectionId>();
}
