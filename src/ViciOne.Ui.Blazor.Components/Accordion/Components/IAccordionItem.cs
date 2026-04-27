using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Accordion.Components;

/// <summary>
/// Represents a single collapsible item within an <see cref="Accordion"/>.
/// Renders a clickable header and an optional content area that is shown when expanded.
/// </summary>
public interface IAccordionItem : IHasIcon
{
    /// <summary>
    /// Gets the display text rendered in the item header.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the HTML <c>title</c> attribute applied to the root element.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Gets whether the item is currently expanded and its content is visible.
    /// </summary>
    bool Expanded { get; }

    /// <summary>
    /// Gets the additional CSS class applied to the root element.
    /// </summary>
    string? CssClass { get; }
}
