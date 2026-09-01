using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Models;

public sealed partial class BreadcrumbItemTests
{
    public sealed class RemoveChildren
    {
        [Fact]
        public void Should_remove_children_from_previous_parent_and_null_parent()
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
}
