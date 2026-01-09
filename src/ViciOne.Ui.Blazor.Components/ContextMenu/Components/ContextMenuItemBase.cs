using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ContextMenu.Models;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Components;

/// <summary>
/// Base class for context menu items
/// </summary>
public class ContextMenuItemBase : ComponentBase
{
    /// <summary>
    /// Uppermost context menu in the logical tree of context menus
    /// </summary>
    [CascadingParameter(Name = Constants.CascadingParameterNames.RootContextMenu)]
    internal IContextMenu RootContextMenu { get; set; } = default!;

    /// <summary>
    /// Context menu that contains the context menu item
    /// </summary>
    [CascadingParameter(Name = Constants.CascadingParameterNames.ParentContextMenu)]
    internal IContextMenu ParentContextMenu { get; set; } = default!;

    /// <summary>
    /// Cascaded ApplicableTo configuration
    /// </summary>
    [CascadingParameter] protected ContextMenuApplicableTo? InheritedApplicableTo { get; set; }

    /// <summary>
    /// Cascaded item filter
    /// </summary>
    [CascadingParameter] protected ContextMenuItemFilter? ItemFilter { get; set; }

    /// <summary>
    /// ApplicableTo configuration that overrides <see cref="InheritedApplicableTo"/>
    /// </summary>
    [Parameter] public ContextMenuApplicableTo? ApplicableTo { get; set; }

    /// <summary>
    /// True when item should be visible, otherwise false
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

    /// <summary>
    /// ApplicableTo configuration provided to descendant components of the context menu
    /// </summary>
    protected ContextMenuApplicableTo? CascadingApplicableTo => ApplicableTo ?? InheritedApplicableTo;

    /// <summary>
    /// True when <see cref="ItemFilter"/> matches with <see cref="CascadingApplicableTo"/>, otherwise false
    /// </summary>
    protected bool ShouldBeShown()
    {
        bool hasMatchingApplicableTo;
        var applicableTo = CascadingApplicableTo;

        if (ItemFilter is null)
        {
            hasMatchingApplicableTo = applicableTo is null;
        }
        else
        {
            if (ItemFilter.ApplicableTo?.Any() != true)
            {
                hasMatchingApplicableTo = applicableTo?.Types.Any() != true;
            }
            else
            {
                if (applicableTo is not null)
                    hasMatchingApplicableTo = ItemFilter.ApplicableTo.All(t => applicableTo.Types.Contains(t));
                else
                    hasMatchingApplicableTo = false;
            }
        }

        return hasMatchingApplicableTo && Visible;
    }
}
