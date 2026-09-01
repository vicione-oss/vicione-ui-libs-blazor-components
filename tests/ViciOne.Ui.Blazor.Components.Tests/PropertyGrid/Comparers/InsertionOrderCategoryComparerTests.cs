using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class InsertionOrderCategoryComparerTests
{
    [Fact]
    public void Compare_always_returns_zero()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddInsertionOrderCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IInsertionOrderCategoryComparer>();

        // Act, Assert
        comparer.Compare("Alpha", "Zebra").Should().Be(0);
        comparer.Compare("Zebra", "Alpha").Should().Be(0);
        comparer.Compare("Alpha", "Alpha").Should().Be(0);
    }

    [Fact]
    public void Sort_preserves_insertion_order()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddInsertionOrderCategoryComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IInsertionOrderCategoryComparer>();

        string[] categories = ["Zebra", "Alpha", "Mango"];

        // Act
        var sorted = categories.Order(comparer).ToList();

        // Assert
        sorted.Should().Equal("Zebra", "Alpha", "Mango");
    }
}
