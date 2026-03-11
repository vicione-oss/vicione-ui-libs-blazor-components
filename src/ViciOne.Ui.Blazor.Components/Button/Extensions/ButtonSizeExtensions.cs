using ViciOne.Ui.Blazor.Components.Button.Enums;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.Button.Extensions;

/// <summary>
/// Extension methods for <see cref="ButtonSize"/>.
/// </summary>
/// <remarks>
/// Provides mappings to icon sizes and CSS modifier classes used by the UI theme.
/// </remarks>
public static class ButtonSizeExtensions
{
    /// <summary>
    /// Maps the given <paramref name="buttonSize"/> to a corresponding <see cref="MonochromeIconSize"/>.
    /// </summary>
    /// <param name="buttonSize">The logical button size.</param>
    /// <returns>
    /// The associated <see cref="MonochromeIconSize"/>:
    /// <list type="bullet">
    ///   <item><description><c>Small</c> → <see cref="MonochromeIconSize.Small"/></description></item>
    ///   <item><description><c>Medium</c> → <see cref="MonochromeIconSize.SmallMedium"/></description></item>
    ///   <item><description><c>Large</c> → <see cref="MonochromeIconSize.Medium"/></description></item>
    /// </list>
    /// </returns>
    public static MonochromeIconSize ToMonochromeIconSize(this ButtonSize buttonSize)
    {
        if (buttonSize == ButtonSize.Small)
            return MonochromeIconSize.Small;
        else if (buttonSize == ButtonSize.Medium)
            return MonochromeIconSize.SmallMedium;
        else if (buttonSize == ButtonSize.Large)
            return MonochromeIconSize.Medium;
        else
            throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the CSS modifier class for the given <paramref name="size"/> in dash-case form.
    /// </summary>
    /// <param name="size">The logical button size.</param>
    /// <returns>
    /// A class name in the form <c>button--{size}</c>, e.g., <c>button--small</c>, <c>button--medium</c>, <c>button--large</c>.
    /// </returns>
    internal static string ToModifierCssClass(this ButtonSize size)
        => $"button--{size.GetName().ToDashCase()}";
}
