using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class ReverseAlphabeticalPropertyComparerTests
{
    private static IPropertyGridItem CreateItem(string displayName)
    {
        var item = Substitute.For<IPropertyGridItem>();
        item.DisplayName.Returns(displayName);
        return item;
    }

    [Fact]
    public void Compare_returns_positive_when_first_comes_before_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Alpha"), CreateItem("Zebra"));

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void Compare_returns_negative_when_first_comes_after_second_alphabetically()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Zebra"), CreateItem("Alpha"));

        // Assert
        result.Should().BeNegative();
    }

    [Fact]
    public void Compare_returns_zero_for_equal_display_names()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalPropertyComparer>();

        // Act
        var result = comparer.Compare(CreateItem("Alpha"), CreateItem("Alpha"));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Sort_orders_items_z_to_a()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddReverseAlphabeticalPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IReverseAlphabeticalPropertyComparer>();

        IPropertyGridItem[] items =
        [
            CreateItem("Alpha"),
            CreateItem("Zebra"),
            CreateItem("Mango"),
        ];

        // Act
        var sorted = items.Order(comparer).Select(i => i.DisplayName).ToList();

        // Assert
        sorted.Should().Equal("Zebra", "Mango", "Alpha");
    }
}
