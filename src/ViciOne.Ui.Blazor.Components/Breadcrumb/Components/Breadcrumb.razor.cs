using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Services;

namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Components;

/// <summary>
/// A component that renders a navigation trail based on a hierarchical item structure.
/// It shows the path from the root to the specified current item as a breadcrumb.
/// In addition, the children of a breadcrumb item are selectable via click on the associated item separator.
/// </summary>
public sealed partial class Breadcrumb : ComponentBase
{
    private double? _lastShownPixel;
    private readonly List<BreadcrumbItemContext> _itemContexts = [];

    /// <summary>
    /// Gets or sets the deepest item in the current navigation path.
    /// The component uses this item's ancestors to build the full breadcrumb trail.
    /// </summary>
    [Parameter, EditorRequired]
    public required BreadcrumbItem CurrentItem { get; set; } = default!;

    /// <summary>
    /// A callback that is invoked when an item is clicked.
    /// The event argument provides the specific <see cref="BreadcrumbItem"/> that was selected.
    /// </summary>
    [Parameter]
    public EventCallback<BreadcrumbItem> OnItemClick { get; set; }

    [Inject] private IHtmlElementHelper HtmlElementHelper { get; init; } = default!;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var newItemContexts = CurrentItem.GetAncestorsIncludingSelf()
            .ConvertAll(breadcrumbItem => new BreadcrumbItemContext { Instance = breadcrumbItem });

        // Adopt ElementReference from existing item contexts to new item contexts as
        // items are rendered with @key="item" instead of @key="itemContext" resulting in Blazor not updating
        // BreadcrumbItemContext.ElementReference for existing items.
        foreach (var newItemContext in newItemContexts)
        {
            var existingItemContext = _itemContexts.FirstOrDefault(i => i.Instance == newItemContext.Instance);
            if (existingItemContext != null)
                newItemContext.ElementReference = existingItemContext.ElementReference;
        }

        _itemContexts.Clear();
        _itemContexts.AddRange(newItemContexts);
    }

    private async Task HandleScrollAsync(int targetScrollStep)
    {
        targetScrollStep = int.Clamp(targetScrollStep, 0, _itemContexts.Count - 1);

        var elementReferences = _itemContexts.Take(targetScrollStep + 1)
            .Select(i => i.ElementReference)
            .OfType<ElementReference>()
            .ToList();

        var boundingClientRects = await HtmlElementHelper.GetBoundingClientRectsAsync(elementReferences);

        _lastShownPixel = boundingClientRects.Sum(s => s.Width);
    }

    private BreadcrumbItem? GetNextBreadcrumbItem(BreadcrumbItemContext itemContext)
    {
        var index = _itemContexts.IndexOf(itemContext);

        return _itemContexts.Count > index + 1 ? _itemContexts[index + 1].Instance : null;
    }

    private async Task ItemClickAsync(BreadcrumbItem item)
    {
        if (OnItemClick.HasDelegate)
            await OnItemClick.InvokeAsync(item);
    }
}
