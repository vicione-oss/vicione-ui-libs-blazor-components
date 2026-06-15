using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("camelCaseString", "camel-case-string")]
    [InlineData("CamelCaseString", "camel-case-string")]
    public void Assert_to_dash_case_result(string givenValue, string expectedValue)
    {
        // Act
        var result = givenValue.ToDashCase();

        // Assert
        result.Should().Be(expectedValue);
    }
}
