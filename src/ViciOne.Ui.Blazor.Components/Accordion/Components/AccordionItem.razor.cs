using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Accordion.Models;

namespace ViciOne.Ui.Blazor.Components.Accordion.Components;

/// <inheritdoc cref="IAccordionItem"/>
public sealed partial class AccordionItem : ComponentBase, IAccordionItem
{
    /// <summary>
    /// Gets or sets the display text rendered in the item header.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the HTML <c>title</c> attribute applied to the root element.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether the item is currently expanded and its content is visible.
    /// </summary>
    [Parameter]
    public bool Expanded { get; set; }

    /// <summary>
    /// Raised when <see cref="Expanded"/> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ExpandedChanged { get; set; }

    /// <summary>
    /// Gets or sets an additional CSS class applied to the root element.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public string? IconCssClass { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public Uri? IconUrl { get; set; }

    /// <inheritdoc/>
    /// <remarks>The icon size should be 24 x 24 pixels.</remarks>
    [Parameter]
    public string? IconData { get; set; }

    /// <summary>
    /// Gets or sets the content rendered inside the item when it is expanded.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback invoked before this item is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionItemCancelEventArgs> OnBeforeExpand { get; set; }

    /// <summary>
    /// A callback invoked before this item is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<AccordionItemCancelEventArgs> OnBeforeCollapse { get; set; }

    internal async Task ExpandAsync()
    {
        var args = new AccordionItemCancelEventArgs { Sender = this };

        if (OnBeforeExpand.HasDelegate)
            await OnBeforeExpand.InvokeAsync(args);

        if (!args.Cancel)
        {
            Expanded = true;

            if (ExpandedChanged.HasDelegate)
                await ExpandedChanged.InvokeAsync(Expanded);
        }
    }

    internal async Task CollapseAsync()
    {
        var args = new AccordionItemCancelEventArgs { Sender = this };

        if (OnBeforeCollapse.HasDelegate)
            await OnBeforeCollapse.InvokeAsync(args);

        if (!args.Cancel)
        {
            Expanded = false;

            if (ExpandedChanged.HasDelegate)
                await ExpandedChanged.InvokeAsync(Expanded);
        }
    }

    private async Task OnHeaderClickAsync()
    {
        if (Expanded)
            await CollapseAsync();
        else
            await ExpandAsync();
    }
}
