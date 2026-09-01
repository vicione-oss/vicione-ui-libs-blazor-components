using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class HasValueDifferentFromDefaultValue
    {
        [Theory]
        [InlineData("Lorem", "Ipsum", "Dolor", false)]
        [InlineData("Lorem", "Dolor", "Dolor", null)]
        [InlineData("Dolor", "Dolor", "Dolor", true)]
        public void Assert_result(
            string name1, string name2, string defaultName, bool? expectedResult)
        {
            // Arrange
            var instance1 = new Foo { Name = name1 };
            var instance2 = new Foo { Name = name2 };

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                HasValueDifferentFromDefaultValue =
                    (instance, defaultValue) => instance.Name.Equals(defaultValue, StringComparison.Ordinal)
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedHasValueDifferentFromDefaultValue(
                [instance1, instance2], defaultName);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void Should_return_null_when_has_value_different_from_default_value_is_not_defined()
        {
            // Arrange
            var instance = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedHasValueDifferentFromDefaultValue([instance], string.Empty);

            // Assert
            result.Should().BeNull();
        }
    }
}
