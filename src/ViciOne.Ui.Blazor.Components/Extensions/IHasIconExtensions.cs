using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Extensions;

internal static class IHasIconExtensions
{
    public static bool IsAnyIconParameterSet(this IHasIcon hasIcon)
        => !string.IsNullOrWhiteSpace(hasIcon.IconCssClass) || hasIcon.IconUrl is not null || !string.IsNullOrWhiteSpace(hasIcon.IconData);
}
