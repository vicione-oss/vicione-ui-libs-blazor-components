using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class AlphabeticalCategoryComparerTests
{
    [Fact]
    public void Compare_returns_negative_when_first_comes_before_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Alpha", "Zebra");

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_returns_positive_when_first_comes_after_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Zebra", "Alpha");

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void Compare_returns_zero_for_equal_category_names()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("Alpha", "Alpha");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_is_case_insensitive()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalCategoryComparer>();

        // Act
        var result = comparer.Compare("alpha", "Alpha");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Sort_orders_categories_a_to_z()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalCategoryComparer>();
        string[] categories = ["Zebra", "Alpha", "Mango"];

        // Act
        var sorted = categories.Order(comparer).ToList();

        // Assert
        sorted.Should().Equal("Alpha", "Mango", "Zebra");
    }
}
