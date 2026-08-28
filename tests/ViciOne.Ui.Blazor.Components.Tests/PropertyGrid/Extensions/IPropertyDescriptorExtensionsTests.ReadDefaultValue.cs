using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class ReadDefaultValue
    {
        private readonly IEqualityComparer<string?> _valueEqualityComparer = EqualityComparer<string?>.Default;

        [Theory]
        [InlineData(null, null, true, null)]
        [InlineData("", null, false, null)]
        [InlineData("", "", true, "")]
        [InlineData(null, "", false, null)]
        public void Assert_result(string? defaultValue1, string? defaultValue2,
            bool shouldReturnValueOf, string? expectedValue)
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string?>
            {
                Name = nameof(Foo.Description),
                GetDefaultValue = instance =>
                {
                    if (instance == instance1)
                        return defaultValue1;

                    return defaultValue2;
                },
                GetValue = instance => instance.Description
            };

            // Act
            var result = propertyDescriptor.ReadDefaultValue([instance1, instance2], _valueEqualityComparer);

            // Assert
            if (shouldReturnValueOf)
                result.Should().Match<ValueOf<string?>>(value => value.Value == expectedValue);
            else
                result.Should().BeNull();
        }

        [Fact]
        public void Should_return_null_when_no_instances_are_passed()
        {
            // Arrange
            var propertyDescriptor = new PropertyDescriptor<Foo, string?>
            {
                Name = nameof(Foo.Description),
                GetDefaultValue = _ => "Lorem ipsum",
                GetValue = instance => instance.Description
            };

            // Act
            var result = propertyDescriptor.ReadDefaultValue([], _valueEqualityComparer);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_return_null_when_get_default_value_assignment_is_missing()
        {
            // Arrange
            var propertyDescriptor = new PropertyDescriptor<Foo, string?>
            {
                Name = nameof(Foo.Description),
                GetValue = instance => instance.Description
            };

            // Act
            var result = propertyDescriptor.ReadDefaultValue([], _valueEqualityComparer);

            // Assert
            result.Should().BeNull();
        }
    }
}
