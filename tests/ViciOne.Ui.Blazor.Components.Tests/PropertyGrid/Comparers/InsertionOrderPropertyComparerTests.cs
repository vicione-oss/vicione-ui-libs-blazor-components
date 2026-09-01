using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Comparers;

public sealed class InsertionOrderPropertyComparerTests
{
    private static IPropertyGridItem CreateItem(string displayName)
    {
        var item = Substitute.For<IPropertyGridItem>();
        item.DisplayName.Returns(displayName);
        return item;
    }

    [Fact]
    public void Compare_always_returns_zero()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddInsertionOrderPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IInsertionOrderPropertyComparer>();

        // Act, Assert
        comparer.Compare(CreateItem("Alpha"), CreateItem("Zebra")).Should().Be(0);
        comparer.Compare(CreateItem("Zebra"), CreateItem("Alpha")).Should().Be(0);
        comparer.Compare(CreateItem("Alpha"), CreateItem("Alpha")).Should().Be(0);
    }

    [Fact]
    public void Sort_preserves_insertion_order()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddInsertionOrderPropertyComparer();

        using var serviceProvider = services.BuildServiceProvider();

        var comparer = serviceProvider.GetRequiredService<IInsertionOrderPropertyComparer>();

        IPropertyGridItem[] items =
        [
            CreateItem("Zebra"),
            CreateItem("Alpha"),
            CreateItem("Mango"),
        ];

        // Act
        var sorted = items.Order(comparer).Select(i => i.DisplayName).ToList();

        // Assert
        sorted.Should().Equal("Zebra", "Alpha", "Mango");
    }
}
