using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("camelCaseString", "camel-case-string")]
    [InlineData("CamelCaseString", "camel-case-string")]
    public void AssertToDashCaseResult(string givenValue, string expectedValue)
    {
        // Act
        var result = givenValue.ToDashCase();

        // Assert
        result.Should().Be(expectedValue);
    }
}
