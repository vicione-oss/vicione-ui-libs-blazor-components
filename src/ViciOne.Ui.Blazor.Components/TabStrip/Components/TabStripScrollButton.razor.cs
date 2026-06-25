using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Components;

/// <summary>
/// A scroll button used within <see cref="TabStrip"/> to navigate overflowing tabs.
/// </summary>
public sealed partial class TabStripScrollButton : ComponentBase
{
    private MonochromeIconName _iconName;
    private MonochromeIconSize _iconSize;

    /// <summary>
    /// The scroll direction of this button.
    /// </summary>
    [Parameter, EditorRequired]
    public ScrollDirection Direction { get; set; }

    /// <summary>
    /// The size of the surrounding <see cref="TabStrip"/>, used to derive the icon size.
    /// </summary>
    [Parameter, EditorRequired]
    public TabSize TabSize { get; set; }

    /// <summary>
    /// Whether the button is currently active (i.e. scrolling in that direction is possible).
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// Raised when the button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _iconName = Direction == ScrollDirection.Left ? MonochromeIconName.ExpanderLightLeft : MonochromeIconName.ExpanderLightRight;

        _iconSize = TabSize == TabSize.Small ? MonochromeIconSize.SmallMedium : MonochromeIconSize.Medium;
    }
}
