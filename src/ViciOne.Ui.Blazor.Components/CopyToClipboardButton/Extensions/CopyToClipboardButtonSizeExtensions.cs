using ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Extensions;

internal static class CopyToClipboardButtonSizeExtensions
{
    public static string ToModifierCssClass(this CopyToClipboardButtonSize size)
        => $"button--{size.GetName().ToDashCase()}";

    public static MonochromeIconSize ToMonochromeIconSize(this CopyToClipboardButtonSize size)
    {
        if (size == CopyToClipboardButtonSize.Small)
            return MonochromeIconSize.Small;

        if (size == CopyToClipboardButtonSize.Medium)
            return MonochromeIconSize.SmallMedium;

        if (size == CopyToClipboardButtonSize.Large)
            return MonochromeIconSize.Medium;

        throw new NotImplementedException();
    }
}
