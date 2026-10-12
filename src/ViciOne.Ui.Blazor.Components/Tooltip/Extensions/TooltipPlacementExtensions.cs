using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Enums;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

internal static class TooltipPlacementExtensions
{
    public static string ToModifierCssClass(this TooltipPlacement placement)
        => $"tooltip--{placement.GetName().ToDashCase()}";
}
