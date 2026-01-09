using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static class IHasIconExtensions
{
    public static bool IsAnyIconParameterSet(this IHasIcon component)
        => component.IconCssClass is not null || component.IconUrl is not null || !string.IsNullOrWhiteSpace(component.IconData);
}
