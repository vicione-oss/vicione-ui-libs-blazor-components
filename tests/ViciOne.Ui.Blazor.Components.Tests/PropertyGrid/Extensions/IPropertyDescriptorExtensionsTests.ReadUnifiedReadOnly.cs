using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class ReadOnly
    {
        [Fact]
        public void Should_not_be_unified_read_only_when_read_only_is_not_assigned()
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
            var result = propertyDescriptor.ReadUnifiedReadOnly([instance1, instance2]);

            // Assert
            result.Should().Be(false);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void Assert_unified_read_only(bool firstInstanceNameReadOnly, bool secondInstanceNameReadOnly, bool expectedResult)
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                ReadOnly = instance =>
                    (firstInstanceNameReadOnly && instance == instance1) ||
                    (secondInstanceNameReadOnly && instance == instance2)
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedReadOnly([instance1, instance2]);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
