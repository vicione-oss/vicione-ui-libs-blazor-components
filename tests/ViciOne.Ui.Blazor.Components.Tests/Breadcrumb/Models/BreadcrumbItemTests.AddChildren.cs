using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Models;

public sealed partial class BreadcrumbItemTests
{
    public sealed class AddChildren
    {
        [Fact]
        public void Should_add_all_children_to_new_parent_and_remove_from_old_parents()
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
    }
}
