using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class Visible
    {
        [Fact]
        public void Should_be_visible_when_visible_is_not_assigned()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedVisible([instance1, instance2]);

            // Assert
            result.Should().Be(true);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void Assert_unified_visible(bool firstInstanceNameVisible, bool secondInstanceNameVisible, bool expectedResult)
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                Visible = instance =>
                    (firstInstanceNameVisible && instance == instance1) ||
                    (secondInstanceNameVisible && instance == instance2)
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedVisible([instance1, instance2]);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
