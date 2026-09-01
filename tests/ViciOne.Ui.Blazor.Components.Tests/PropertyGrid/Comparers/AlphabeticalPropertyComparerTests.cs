using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class AlphabeticalPropertyComparerTests
{
    private static IPropertyGridItem CreateItem(string displayName)
    {
        var item = Substitute.For<IPropertyGridItem>();
        item.DisplayName.Returns(displayName);
        return item;
    }

    [Fact]
    public void Compare_returns_negative_when_first_comes_before_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Alpha"), CreateItem("Zebra"));

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_returns_positive_when_first_comes_after_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Zebra"), CreateItem("Alpha"));

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void Compare_returns_zero_for_equal_display_names()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Alpha"), CreateItem("Alpha"));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_is_case_insensitive()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("alpha"), CreateItem("Alpha"));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Sort_orders_items_a_to_z()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IAlphabeticalPropertyComparer>();

        IPropertyGridItem[] items =
        [
            CreateItem("Zebra"),
            CreateItem("Alpha"),
            CreateItem("Mango"),
        ];

        // Act
        var sorted = items.Order(comparer).Select(i => i.DisplayName).ToList();

        // Assert
        sorted.Should().Equal("Alpha", "Mango", "Zebra");
    }
}
