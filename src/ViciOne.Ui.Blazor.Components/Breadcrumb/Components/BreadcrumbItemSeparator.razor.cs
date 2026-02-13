using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Components;

/// <summary>
/// A component that renders a breadcrumb separator.
/// It can be clicked to display a pop-up showing associated <see cref="Items"/>.
/// </summary>
public sealed partial class BreadcrumbItemSeparator : ComponentBase
{
    private static readonly MonochromeIconSize s_iconSize = MonochromeIconSize.Small;

    private readonly string _icon = MonochromeIconName.ExpanderLightRight.GetCssClasses(s_iconSize).ToSpaceSeparated();

    /// <summary>
    /// Gets or sets the collection of breadcrumb items to be displayed when the separator
    /// is expanded.
    /// </summary>
    [Parameter, EditorRequired]
    public required IReadOnlyCollection<BreadcrumbItem> Items { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb item that is currently active.
    /// Can be <see langword="null"/> if no item is active.
    /// </summary>
    /// <remarks>
    /// The pop-up that appears when you click on the separator visually highlights
    /// an active element.
    /// </remarks>
    [Parameter, EditorRequired]
    public required BreadcrumbItem? ActiveItem { get; set; }

    /// <summary>
    /// A callback that is invoked when an individual item within the popup is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<BreadcrumbItem> OnItemClick { get; set; }

    private bool Expanded { get; set; }

    private void IconClick()
        => Expanded = true;

    private void PointerLeave()
        => Expanded = false;

    private async Task ItemClickAsync(BreadcrumbItem item)
    {
        if (OnItemClick.HasDelegate)
            await OnItemClick.InvokeAsync(item);
    }
}
