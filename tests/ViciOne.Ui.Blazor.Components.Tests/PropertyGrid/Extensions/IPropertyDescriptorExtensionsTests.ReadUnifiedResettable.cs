using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class Resettable
    {
        [Fact]
        public void Should_invoke_resettable_for_each_instance()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var resettableValueIterations = new List<Foo>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ResetValue = _ => { },
                Resettable = instance =>
                {
                    resettableValueIterations.Add(instance);

                    return true;
                }
            };

            // Act
            propertyDescriptor.ReadUnifiedResettable([instance1, instance2]);

            // Assert
            resettableValueIterations.Should().BeEquivalentTo([instance1, instance2]);
        }

        [Fact]
        public void Should_return_false_when_reset_value_assignment_is_missing()
        {
            // Arrange
            var instance1 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                Resettable = _ => default
            };

            // Act
            var resettable = propertyDescriptor.ReadUnifiedResettable([instance1]);

            // Assert
            resettable.Should().BeFalse();
        }

        [Fact]
        public void Should_return_true_when_reset_value_is_defined_but_resettable_is_not()
        {
            // Arrange
            var instance1 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ResetValue = _ => { }
            };

            // Act
            var resettable = propertyDescriptor.ReadUnifiedResettable([instance1]);

            // Assert
            resettable.Should().BeTrue();
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void Assert_result_based_on_resettable_state(bool instance1Resettable, bool instance2Resettable,
            bool expectedResult)
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ResetValue = _ => { },
                Resettable = instance =>
                {
                    if (instance == instance1)
                        return instance1Resettable;

                    if (instance == instance2)
                        return instance2Resettable;

                    return false;
                }
            };

            // Act
            var resettable = propertyDescriptor.ReadUnifiedResettable([instance1, instance2]);

            // Assert
            resettable.Should().Be(expectedResult);
        }
    }
}
