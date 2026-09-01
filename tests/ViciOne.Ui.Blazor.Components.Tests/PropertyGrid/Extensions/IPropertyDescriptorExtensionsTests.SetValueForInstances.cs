using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class SetValueForInstances
    {
        [Fact]
        public void Should_set_priority_of_each_instance()
        {
            // Arrange
            var instance1 = new Foo { Priority = 1 };
            var instance2 = new Foo { Priority = 2 };

            var setValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, int>
            {
                Name = nameof(Foo.Priority),
                GetValue = instance => instance.Priority,
                SetValue = (instance, value) =>
                {
                    instance.Priority = value;

                    setValueIterations.Add(instance);
                }
            };

            var exceptions = new List<Exception>();

            // Act
            propertyDescriptor.SetValueForInstances([instance1, instance2], value: 3, exceptions);

            // Assert
            instance1.Priority.Should().Be(3);
            instance2.Priority.Should().Be(3);

            setValueIterations.Should().BeEquivalentTo([instance1, instance2]);
        }
    }
}
