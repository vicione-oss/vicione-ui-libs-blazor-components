using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class SetValueFromMap
    {
        [Fact]
        public void Should_set_name_of_each_instance()
        {
            // Arrange
            var instance1 = new Foo { Name = "Lorem" };
            var instance2 = new Foo { Name = "Ipsum" };

            var setValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) =>
                {
                    instance.Name = value;

                    setValueIterations.Add(instance);
                }
            };

            var valueMap = new Dictionary<ValueKey, string>();
            propertyDescriptor.FillValueMap([instance1, instance2], valueMap);

            instance1.Name = "Dolor";
            instance2.Name = "Sit";

            var exceptions = new List<Exception>();

            // Act
            propertyDescriptor.SetValueFromMap([instance1, instance2], valueMap, exceptions);

            // Assert
            instance1.Name.Should().Be("Lorem");
            instance2.Name.Should().Be("Ipsum");

            setValueIterations.Should().BeEquivalentTo([instance1, instance2]);
        }

        [Fact]
        public void Should_not_set_name_when_not_enabled()
        {
            // Arrange
            var instance = new Foo { Name = "Lorem" };

            var setValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                Enabled = _ => false,
                GetValue = instance => instance.Name,
                SetValue = (instance, _) => setValueIterations.Add(instance)
            };

            var valueMap = new Dictionary<ValueKey, string>();
            propertyDescriptor.FillValueMap([instance], valueMap);

            var exceptions = new List<Exception>();

            // Act
            propertyDescriptor.SetValueFromMap([instance], valueMap, exceptions);

            // Assert
            setValueIterations.Should().BeEmpty();
        }

        [Fact]
        public void Should_not_set_name_when_read_only()
        {
            // Arrange
            var instance = new Foo { Name = "Lorem" };

            var setValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                ReadOnly = _ => true,
                GetValue = instance => instance.Name,
                SetValue = (instance, _) => setValueIterations.Add(instance)
            };

            var valueMap = new Dictionary<ValueKey, string>();
            propertyDescriptor.FillValueMap([instance], valueMap);

            var exceptions = new List<Exception>();

            // Act
            propertyDescriptor.SetValueFromMap([instance], valueMap, exceptions);

            // Assert
            setValueIterations.Should().BeEmpty();
        }

        [Fact]
        public void Should_not_set_name_when_set_value_is_not_defined()
        {
            // Arrange
            var instance = new Foo { Name = "Lorem" };

            var setValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name
            };

            var valueMap = new Dictionary<ValueKey, string>();
            propertyDescriptor.FillValueMap([instance], valueMap);

            var exceptions = new List<Exception>();

            instance.Name = "Dolor";

            // Act
            propertyDescriptor.SetValueFromMap([instance], valueMap, exceptions);

            // Assert
            instance.Name.Should().Be("Dolor");
        }
    }
}
