using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Models;

public sealed partial class BreadcrumbItemTests
{
    public sealed class RemoveChild
    {
        [Fact]
        public void Should_remove_child_from_previous_parent_and_null_parent()
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
        public void Should_return_false_if_is_no_child()
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
    }
}
