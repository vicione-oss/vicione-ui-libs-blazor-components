using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class CanSetValue
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void Assert_result_based_on_given_enabled_state_per_instance(bool enabled1,
            bool enabled2, bool expectedResult)
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
                {
                    if (instance == instance1)
                        return enabled1;

                    return enabled2;
                }
            };

            // Act
            var result = propertyDescriptor.CanSetValue([instance1, instance2]);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        public void Assert_result_based_on_given_read_only_state_per_instance(bool readOnly1,
            bool readOnly2, bool expectedResult)
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
                {
                    if (instance == instance1)
                        return readOnly1;

                    return readOnly2;
                }
            };

            // Act
            var result = propertyDescriptor.CanSetValue([instance1, instance2]);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void Should_return_false_when_set_value_assignment_is_missing()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name
            };

            // Act
            var result = propertyDescriptor.CanSetValue([instance1, instance2]);

            // Assert
            result.Should().BeFalse();
        }
    }
}
