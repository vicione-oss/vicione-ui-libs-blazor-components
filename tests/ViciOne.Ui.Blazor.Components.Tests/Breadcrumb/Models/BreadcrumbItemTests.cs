using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Models;

public sealed class BreadcrumbItemTests
{
    [Fact]
    public void AddChild_should_add_to_new_parent_and_remove_child_from_previous_parent()
    {
        // Arrange
        var oldParent = new BreadcrumbItem { Name = "oldParent" };
        var newParent = new BreadcrumbItem { Name = "newParent" };
        var item = new BreadcrumbItem { Name = "item" };
        var oldSibling = new BreadcrumbItem { Name = "oldSibling" };
        var newSibling = new BreadcrumbItem { Name = "newSibling" };
        oldParent.AddChild(item);
        oldParent.AddChild(oldSibling);
        newParent.AddChild(newSibling);

        // Act
        newParent.AddChild(item);

        // Assert
        newParent.Children.Should().BeEquivalentTo([item, newSibling]);
        oldParent.Children.Should().BeEquivalentTo([oldSibling]);
    }

    [Fact]
    public void AddChildren_should_add_all_children_to_new_parent_and_remove_from_old_parents()
    {
        // Arrange
        var newParent = new BreadcrumbItem { Name = "newParent" };
        var oldParent1 = new BreadcrumbItem { Name = "oldParent1" };
        var oldParent2 = new BreadcrumbItem { Name = "oldParent2" };
        var item1 = new BreadcrumbItem { Name = "noParent" };
        var item2 = new BreadcrumbItem { Name = "childOfOldParent1" };
        var item3 = new BreadcrumbItem { Name = "childOfOldParent1" };
        var item4 = new BreadcrumbItem { Name = "childOfOldParent2" };
        oldParent1.AddChildren([item2, item3]);
        oldParent2.AddChild(item4);

        // Act
        newParent.AddChildren([item1, item2, item4]);

        // Assert
        oldParent1.Children.Should().BeEquivalentTo([item3]);
        item3.Parent.Should().Be(oldParent1);

        oldParent2.Children.Should().BeEmpty();

        newParent.Children.Should().BeEquivalentTo([item1, item2, item4]);
        item1.Parent.Should().Be(newParent);
        item2.Parent.Should().Be(newParent);
        item4.Parent.Should().Be(newParent);
    }

    [Fact]
    public void RemoveChild_should_remove_child_from_previous_parent_and_null_parent()
    {
        // Arrange
        var parent = new BreadcrumbItem { Name = "parent" };
        var item = new BreadcrumbItem { Name = "item" };
        var sibling = new BreadcrumbItem { Name = "sibling" };
        parent.AddChild(item);
        parent.AddChild(sibling);

        // Act
        var result = parent.RemoveChild(item);

        // Assert
        parent.Children.Should().BeEquivalentTo([sibling]);
        item.Parent.Should().BeNull();
        result.Should().BeTrue();
    }

    [Fact]
    public void RemoveChild_should_return_false_if_is_no_child()
    {
        // Arrange
        var parent = new BreadcrumbItem { Name = "parent" };
        var otherParent = new BreadcrumbItem { Name = "otherParent" };
        var item = new BreadcrumbItem { Name = "item" };
        var preexistingItem = new BreadcrumbItem { Name = "preexistingItem" };
        otherParent.AddChild(item);
        parent.AddChild(preexistingItem);

        // Act
        var result = parent.RemoveChild(item);

        // Assert
        parent.Children.Should().BeEquivalentTo([preexistingItem]);
        item.Parent.Should().Be(otherParent);
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveChildren_should_remove_children_from_previous_parent_and_null_parent()
    {
        // Arrange
        var parent = new BreadcrumbItem { Name = "parent" };
        var parent2 = new BreadcrumbItem { Name = "parent2" };
        var item1 = new BreadcrumbItem { Name = "keep" };
        var item2 = new BreadcrumbItem { Name = "remove" };
        var item3 = new BreadcrumbItem { Name = "remove" };
        var item4 = new BreadcrumbItem { Name = "noParent" };
        var item5 = new BreadcrumbItem { Name = "childOfParent2" };
        parent.AddChildren([item1, item2, item3]);
        parent2.AddChild(item5);

        // Act
        parent.RemoveChildren([item2, item3, item4, item5]);

        // Assert
        parent.Children.Should().BeEquivalentTo([item1]);
        item1.Parent.Should().Be(parent);

        parent2.Children.Should().BeEquivalentTo([item5]);
        item5.Parent.Should().Be(parent2);

        item2.Parent.Should().BeNull();
        item3.Parent.Should().BeNull();
        item4.Parent.Should().BeNull();
    }
}
