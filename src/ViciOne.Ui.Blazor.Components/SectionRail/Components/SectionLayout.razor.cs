using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.SectionRail.Components.ActionButtons;

namespace ViciOne.Ui.Blazor.Components.SectionRail.Components;

/// <summary>
/// Component that implements a general layout for sections
/// </summary>
public sealed partial class SectionLayout : ComponentBase
{
    /// <summary>
    /// Renders action buttons as part of the header area.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SectionActionButton"/> to render action buttons in a consistent way.
    /// </remarks>
    [Parameter]
    public RenderFragment? ActionButtons { get; set; }

    /// <summary>
    /// Renders tools above <see cref="Content"/>
    /// </summary>
    [Parameter]
    public RenderFragment? Tools { get; set; }

    /// <summary>
    /// Renders the content of the section
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    [CascadingParameter]
    private ISection Section { get; set; } = default!;
}
