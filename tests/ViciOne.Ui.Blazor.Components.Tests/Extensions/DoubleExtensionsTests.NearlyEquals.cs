using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed class DoubleExtensionsTests
{
    public sealed class NearlyEquals
    {
        [Theory]
        [InlineData(0.0, 0.0009, null, true)]
        [InlineData(0.0, -0.0009, null, true)]
        [InlineData(0.0, 0.001, null, false)]
        [InlineData(0.0, -0.001, null, false)]
        [InlineData(0.0, 0.001, 0.1, true)]
        [InlineData(0.0, 0.099, 0.1, true)]
        [InlineData(0.0, 0.1, 0.1, false)]
        public void Without_nullables_behaves_as_expected(double a, double b, double? epsilon, bool expectedResult)
        {
            // Act
            var result = epsilon is null ? a.NearlyEquals(b) : a.NearlyEquals(b, epsilon.Value);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(0.0, 0.0009, null, true)]
        [InlineData(0.0, -0.0009, null, true)]
        [InlineData(0.0, 0.001, null, false)]
        [InlineData(0.0, -0.001, null, false)]
        [InlineData(0.0, 0.001, 0.1, true)]
        [InlineData(0.0, 0.099, 0.1, true)]
        [InlineData(0.0, 0.1, 0.1, false)]
        [InlineData(null, 0.0, null, false)]
        [InlineData(0.0, null, null, false)]
        [InlineData(null, null, null, true)]
        public void With_nullables_behaves_as_expected(double? a, double? b, double? epsilon, bool expectedResult)
        {
            // Act
            var result = epsilon is null ? a.NearlyEquals(b) : a.NearlyEquals(b, epsilon.Value);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
