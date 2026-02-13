namespace ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

/// <summary>
/// Represents an item in a breadcrumb, supporting parent-child relationships.
/// </summary>
public record BreadcrumbItem
{
    private readonly List<BreadcrumbItem> _children = [];

    /// <summary>
    /// Gets or sets the name of this item.
    /// </summary>
    public virtual required string Name { get; set; }

    /// <summary>
    /// Gets the parent item of this item.
    /// </summary>
    /// <returns>
    /// <see langword="null"/> if this is the root item, otherwise the parent of this item.
    /// </returns>
    public BreadcrumbItem? Parent { get; internal set; }

    /// <summary>
    /// Gets the immediate children of this item.
    /// </summary>
    public IReadOnlyCollection<BreadcrumbItem> Children
        => _children;

    /// <summary>
    /// Adds the given <paramref name="items"/> to the set of children.
    /// </summary>
    public void AddChildren(IEnumerable<BreadcrumbItem> items)
    {
        foreach (var item in items)
            AddChild(item);
    }

    /// <summary>
    /// Adds the given <paramref name="item"/> to the set of children.
    /// </summary>
    /// <returns>The current instance on which <see cref="AddChild(BreadcrumbItem)"/> has been called to support chaining.</returns>
    public BreadcrumbItem AddChild(BreadcrumbItem item)
    {
        item.Parent?.RemoveChild(item);

        item.Parent = this;
        _children.Add(item);

        return item;
    }

    /// <summary>
    /// Removes the given <paramref name="items"/> from the set of children.
    /// </summary>
    public void RemoveChildren(IEnumerable<BreadcrumbItem> items)
    {
        foreach (var item in items)
            RemoveChild(item);
    }

    /// <summary>
    /// Removes the given <paramref name="item"/> from the set of children.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if item was found and successfully removed, otherwise <see langword="false"/>.
    /// This method also returns false if item was not found in the list of children.
    /// </returns>
    public bool RemoveChild(BreadcrumbItem item)
    {
        var removed = _children.Remove(item);
        if (removed)
            item.Parent = null;

        return removed;
    }
}
