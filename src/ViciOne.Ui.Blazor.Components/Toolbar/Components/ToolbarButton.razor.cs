using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Toolbar.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// Button inside a <see cref="Toolbar"/> including some commonly used use cases.
/// </summary>
/// <remarks>
/// For advanced use cases which can not be handled by this component, use <see cref="ToolbarContent"/>.
/// </remarks>
public sealed partial class ToolbarButton : ToolbarItemBase
{
    private ElementReference _container;

    /// <inheritdoc/>
    protected override ElementReference Container => _container;

    /// <summary>
    /// Name of the icon the button should display. If <see langword="null"/>, no icon will be shown.
    /// </summary>
    [Parameter]
    public MonochromeIconName? IconName { get; set; }

    /// <summary>
    /// The text displayed next to the icon.
    /// </summary>
    /// <remarks>
    /// Use <see cref="TextVisibility"/> to configure the visiblity of the text.
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Defines visibility of <see cref="Text"/>.
    /// </summary>
    [Parameter]
    public TextVisibility TextVisibility { get; set; } = TextVisibility.Always;

    /// <summary>
    /// Determines if the button is active or not.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }
}
