using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Accordion.Components;

/// <summary>
/// A component that renders a collection of <see cref="AccordionItem"/> instances.
/// </summary>
public sealed partial class Accordion : ComponentBase
{
    /// <summary>
    /// Gets or sets the content containing <see cref="AccordionItem"/> components.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment ChildContent { get; set; }
}
