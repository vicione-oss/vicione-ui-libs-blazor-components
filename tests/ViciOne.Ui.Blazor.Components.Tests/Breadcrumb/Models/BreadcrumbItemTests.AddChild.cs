using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Models;

public sealed partial class BreadcrumbItemTests
{
    public sealed class AddChild
    {
        [Fact]
        public void Should_add_to_new_parent_and_remove_child_from_previous_parent()
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
    }
}
