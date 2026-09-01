using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Services;

public sealed class PropertyValueEqualityComparerProviderTests
{
    internal sealed class GenericPropertyValueEqualityComparer<TPropertyValue> : IPropertyValueEqualityComparer<TPropertyValue>
    {
        private readonly EqualityComparer<TPropertyValue> _defaultEqualityComparer = EqualityComparer<TPropertyValue>.Default;

        public bool Equals(TPropertyValue? x, TPropertyValue? y) => _defaultEqualityComparer.Equals(x, y);
        public int GetHashCode([DisallowNull] TPropertyValue obj) => _defaultEqualityComparer.GetHashCode(obj);
    }

    [Fact]
    public void Should_provide_comparer_for_type()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddPropertyValueEqualityComparer<object, string, GenericPropertyValueEqualityComparer<string>>()
            .AddPropertyValueEqualityComparerProvider<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var propertyValueEqualityComparerProvider =
            serviceProvider.GetRequiredService<IPropertyValueEqualityComparerProvider<object>>();

        var type = typeof(string);
        var propertyValueEqualityCompareType = typeof(GenericPropertyValueEqualityComparer<>).MakeGenericType(type);

        // Act
        var propertyValueEqualityComparer = propertyValueEqualityComparerProvider.GetPropertyValueEqualityComparer(type);

        // Assert
        propertyValueEqualityComparer.Should().NotBeNull().And.BeOfType(propertyValueEqualityCompareType);
    }

    [Fact]
    public void Should_provide_comparer_for_strings()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddPropertyValueEqualityComparer<object, string, GenericPropertyValueEqualityComparer<string>>()
            .AddPropertyValueEqualityComparerProvider<object>();

        using var serviceProvider = services.BuildServiceProvider();

        var propertyValueEqualityComparerProvider =
            serviceProvider.GetRequiredService<IPropertyValueEqualityComparerProvider<object>>();

        // Act
        var propertyValueEqualityComparer = propertyValueEqualityComparerProvider.GetPropertyValueEqualityComparer<string>();

        // Assert
        propertyValueEqualityComparer.Should().NotBeNull().And.BeOfType<GenericPropertyValueEqualityComparer<string>>();
    }
}
