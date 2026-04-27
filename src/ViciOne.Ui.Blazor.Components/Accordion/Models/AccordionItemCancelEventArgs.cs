using ViciOne.Ui.Blazor.Components.Accordion.Components;

namespace ViciOne.Ui.Blazor.Components.Accordion.Models;

/// <summary>
/// Arguments for the <see cref="AccordionItem.OnBeforeExpand"/> and <see cref="AccordionItem.OnBeforeCollapse"/> events.
/// </summary>
public sealed class AccordionItemCancelEventArgs : EventArgs
{
    /// <summary>
    /// Gets the accordion item that raised the event.
    /// </summary>
    public required IAccordionItem Sender { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the expand or collapse action should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }
}
