using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class ReadUnifiedValue
    {
        private readonly IEqualityComparer<string?> _valueEqualityComparer = EqualityComparer<string?>.Default;

        [Theory]
        [InlineData(null, null, true, null)]
        [InlineData("", null, false, null)]
        [InlineData("", "", true, "")]
        [InlineData(null, "", false, null)]
        public void Assert_result(string? description1, string? description2,
            bool shouldReturnValueOf, string? expectedValue)
        {
            // Arrange
            var instance1 = new Foo { Description = description1 };
            var instance2 = new Foo { Description = description2 };

            var propertyDescriptor = new PropertyDescriptor<Foo, string?>
            {
                Name = nameof(Foo.Description),
                GetValue = instance => instance.Description
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedValue([instance1, instance2], _valueEqualityComparer);

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
                GetValue = instance => instance.Description
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedValue([], _valueEqualityComparer);

            // Assert
            result.Should().BeNull();
        }
    }
}
