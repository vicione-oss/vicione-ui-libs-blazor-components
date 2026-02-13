using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

namespace Shared.Pages.Breadcrumb.Component;

public sealed partial class BreadcrumbPage : ComponentBase
{
    private BreadcrumbItem? _lastClickedBreadcrumbItem;
    private BreadcrumbItem _adaptiveTreeItem = new() { Name = "root" };

    private void ItemClick(BreadcrumbItem item)
        => _lastClickedBreadcrumbItem = item;

    private static BreadcrumbItem GetMinimalTree()
        => new() { Name = "Root" };

    private static BreadcrumbItem GetMinimalTreeWithChild()
    {
        var root = new BreadcrumbItem { Name = "Root" };
        var child = new BreadcrumbItem { Name = "Child" };
        return root.AddChild(child);
    }

    private static BreadcrumbItem GetNormalTree()
    {
        var root = new BreadcrumbItem { Name = "Root" };
        var child = new BreadcrumbItem { Name = "Child" };
        var child2 = new BreadcrumbItem { Name = "Child 2" };
        root.AddChild(child);
        root.AddChild(child2);

        var grandChild = new BreadcrumbItem { Name = "Grandchild" };
        var grandChild2 = new BreadcrumbItem { Name = "Grandchild 2" };
        child.AddChild(grandChild);
        child.AddChild(grandChild2);

        return grandChild2;
    }

    private static BreadcrumbItem GetBigTree()
    {
        var current = new BreadcrumbItem { Name = "Root" };
        for (var i = 1; i <= 20; i++)
        {
            var name = "Level" + string.Concat(Enumerable.Repeat("_", i)) + i;
            var next = new BreadcrumbItem { Name = name };

            current.AddChild(next);

            current = next;
        }

        return current;
    }

    private async Task AddClickAsync()
    {
        var item = new BreadcrumbItem { Name = $"Node {RandomNumberGenerator.GetInt32(100, 999)}" };

        _adaptiveTreeItem.AddChild(item);
        _adaptiveTreeItem = item;
    }

    private async Task RemoveClickAsync()
    {
        if (_adaptiveTreeItem.Parent == null)
            return;

        var parent = _adaptiveTreeItem.Parent;
        parent.RemoveChild(_adaptiveTreeItem);

        _adaptiveTreeItem = parent;
    }
}
