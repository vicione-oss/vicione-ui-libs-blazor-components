using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class FillValueMap
    {
        [Fact]
        public void Should_fill_value_map()
        {
            // Arrange
            var instance1 = new Foo { Name = "Lorem" };
            var instance2 = new Foo { Name = "Ipsum" };

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name
            };

            var valueMap = new Dictionary<ValueKey, string>();

            // Act
            propertyDescriptor.FillValueMap([instance1, instance2], valueMap);

            // Assert
            valueMap.Keys.Should().BeEquivalentTo(
                [new ValueKey(propertyDescriptor, instance1), new ValueKey(propertyDescriptor, instance2)]);

            valueMap.Values.Should().BeEquivalentTo([instance1.Name, instance2.Name]);
        }
    }
}
