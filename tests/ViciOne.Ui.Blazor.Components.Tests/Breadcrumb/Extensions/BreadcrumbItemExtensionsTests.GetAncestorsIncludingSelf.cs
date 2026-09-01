using ViciOne.Ui.Blazor.Components.Breadcrumb.Extensions;
using ViciOne.Ui.Blazor.Components.Breadcrumb.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Breadcrumb.Extensions;

public sealed partial class BreadcrumbItemExtensionsTests
{
    public sealed class GetAncestorsIncludingSelf
    {
        [Fact]
        public void Should_return_null_when_item_is_null()
        {
            // Arrange
            BreadcrumbItem? item = null;

            // Act
            var result = item!.GetAncestorsIncludingSelf();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Should_return_only_self_when_no_ancestors_exist()
        {
            // Arrange
            var item = new BreadcrumbItem { Name = "root" };

            // Act
            var result = item.GetAncestorsIncludingSelf();

            // Assert
            result.Single().Should().Be(item);
        }

        [Fact]
        public void Should_return_self_and_ancestors_when_ancestors_exist()
        {
            // Arrange
            var root = new BreadcrumbItem { Name = "root" };
            var cousin = new BreadcrumbItem { Name = "cousin" };
            var parent = new BreadcrumbItem { Name = "parent" };
            var sibling = new BreadcrumbItem { Name = "sibling" };
            var self = new BreadcrumbItem { Name = "self" };
            var child = new BreadcrumbItem { Name = "child" };
            root.AddChild(cousin);
            root.AddChild(parent);
            parent.AddChild(sibling);
            parent.AddChild(self);
            self.AddChild(child);

            // Act
            var result = self.GetAncestorsIncludingSelf();

            // Assert
            result.Should().BeEquivalentTo([root, parent, self]);
        }
    }
}
