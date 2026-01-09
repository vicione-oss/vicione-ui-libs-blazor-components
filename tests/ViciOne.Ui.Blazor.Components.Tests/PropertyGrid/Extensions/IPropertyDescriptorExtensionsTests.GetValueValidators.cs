using AwesomeAssertions;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class ValueValidators
    {
        [Fact]
        public void Should_fetch_value_validators()
        {
            // Arrange
            var valueValidator = new StringMustNotBeEmptyPropertyValueValidator();
            var anotherValueValidator = Substitute.For<IPropertyValueValidator<string>>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ValueValidators = [valueValidator, anotherValueValidator]
            };

            // Act
            var result = propertyDescriptor.GetValueValidators();

            // Assert
            result.Should().BeEquivalentTo([valueValidator, anotherValueValidator]);
        }
    }
}
