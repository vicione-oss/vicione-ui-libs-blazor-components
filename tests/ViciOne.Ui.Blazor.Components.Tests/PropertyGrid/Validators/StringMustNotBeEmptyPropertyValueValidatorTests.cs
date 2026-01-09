using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Validators;

public sealed class StringMustNotBeEmptyPropertyValueValidatorTests
{
    [Theory]
    [InlineData("", "The text entered must not be null or empty.")]
    [InlineData(null, "The text entered must not be null or empty.")]
    [InlineData("Lorem ipsum", null)]
    public void Assert_validate(string? value, string? expectedResult)
    {
        // Arrange
        var validator = new StringMustNotBeEmptyPropertyValueValidator();

        // Act
        var result = validator.Validate(value);

        // Assert
        result.Should().Be(expectedResult);
    }
}
