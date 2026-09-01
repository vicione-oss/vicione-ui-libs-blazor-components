using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class Enabled
    {
        [Fact]
        public void Should_be_enabled_when_enabled_is_not_assigned()
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
            var result = propertyDescriptor.ReadUnifiedEnabled([instance1, instance2]);

            // Assert
            result.Should().Be(true);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void Assert_unified_enabled(bool firstInstanceNameEnabled, bool secondInstanceNameEnabled, bool expectedResult)
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                Enabled = instance =>
                    (firstInstanceNameEnabled && instance == instance1) ||
                    (secondInstanceNameEnabled && instance == instance2)
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedEnabled([instance1, instance2]);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
