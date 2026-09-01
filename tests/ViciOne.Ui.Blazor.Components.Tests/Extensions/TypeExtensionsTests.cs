using ViciOne.Ui.Blazor.Components.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

public sealed class TypeExtensions
{
    [Theory]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(string), false)]
    public void Assert_result_of_is_nullable_value_type(Type type, bool expectedResult)
    {
        // Arrange, Act
        var result = type.IsNullableValueType();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(typeof(int), typeof(int))]
    [InlineData(typeof(int?), typeof(int))]
    [InlineData(typeof(string), typeof(string))]
    public void Assert_result_of_of_make_nonnullable_type(Type givenType, Type expectedType)
    {
        // Arrange, Act
        var result = givenType.MakeNonNullableType();

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(typeof(int), typeof(int?))]
    [InlineData(typeof(int?), typeof(int?))]
    [InlineData(typeof(string), typeof(string))]
    public void Assert_result_of_of_make_nullable_type(Type givenType, Type expectedType)
    {
        // Arrange, Act
        var result = givenType.MakeNullableType();

        // Assert
        result.Should().Be(expectedType);
    }
}
