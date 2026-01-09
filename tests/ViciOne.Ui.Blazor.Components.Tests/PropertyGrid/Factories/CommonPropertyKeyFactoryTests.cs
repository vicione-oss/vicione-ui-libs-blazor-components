using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Factories;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Factories;

public sealed class CommonPropertyKeyFactoryTests
{
    [Fact]
    public void Should_create_common_property_key()
    {
        // Arrange
        var propertyDescriptor = new PropertyDescriptor<Foo, string>
        {
            Category = "String properties",
            Name = nameof(Foo.Name),
            GetValue = i => i.Name
        };

        // Act
        var commonPropertyKey = CommonPropertyKeyFactory.CreateCommonPropertyKey(propertyDescriptor);

        // Assert
        commonPropertyKey.Should().BeOfType<CommonPropertyKey>()
            .Which.Should().Match<CommonPropertyKey>(k =>
                k.Category == propertyDescriptor.Category &&
                k.Name == propertyDescriptor.Name &&
                k.ValueType == propertyDescriptor.ValueType);
    }

    [Fact]
    public void Should_create_common_numeric_property_key()
    {
        // Arrange
        var propertyDescriptor = new NumericPropertyDescriptor<Foo, int, int, int>
        {
            Category = "Numeric properties",
            Name = nameof(Foo.Priority),
            GetValue = i => i.Priority,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Interval = 1
        };

        // Act
        var commonPropertyKey = CommonPropertyKeyFactory.CreateCommonPropertyKey(propertyDescriptor);

        // Assert
        commonPropertyKey.Should().BeOfType<CommonNumericPropertyKey<int, int>>()
            .Which.Should().Match<CommonNumericPropertyKey<int, int>>(k =>
                k.Category == propertyDescriptor.Category &&
                k.Name == propertyDescriptor.Name &&
                k.ValueType == propertyDescriptor.ValueType &&
                k.LimitType == propertyDescriptor.LimitType &&
                k.IntervalType == propertyDescriptor.IntervalType);
    }

    [Fact]
    public void Should_create_common_selection_property_key()
    {
        // Arrange
        var propertyDescriptor = new SelectionPropertyDescriptor<Foo, Direction>
        {
            Category = "Selection properties",
            Name = nameof(Foo.Direction),
            GetValue = i => i.Direction,
            GetSelectableValues = _ => Enum.GetValues<Direction>()
                .Select(direction => new SelectableValue<Direction> { Value = direction, Text = direction.ToString() })
        };

        // Act
        var commonPropertyKey = CommonPropertyKeyFactory.CreateCommonPropertyKey(propertyDescriptor);

        // Assert
        commonPropertyKey.Should().BeOfType<CommonSelectionPropertyKey>()
            .Which.Should().Match<CommonSelectionPropertyKey>(k =>
                k.Category == propertyDescriptor.Category &&
                k.Name == propertyDescriptor.Name &&
                k.ValueType == propertyDescriptor.ValueType);
    }

    private enum Direction { North, South, SouthEast };

    private sealed class Foo
    {
        public required string Name { get; set; }
        public required int Priority { get; set; }
        public required Direction Direction { get; set; }
    }
}
