using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class ReverseAlphabeticalCategoryComparerTests
{
    [Fact]
    public void Compare_returns_positive_when_first_comes_before_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Alpha", "Zebra");

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void Compare_returns_negative_when_first_comes_after_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Zebra", "Alpha");

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_returns_zero_for_equal_category_names()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Alpha", "Alpha");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Sort_orders_categories_z_to_a()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalCategoryComparer>();

        string[] categories = ["Alpha", "Zebra", "Mango"];

        // Act
        var sorted = categories.Order(comparer).ToList();

        // Assert
        sorted.Should().Equal("Zebra", "Mango", "Alpha");
    }
}
