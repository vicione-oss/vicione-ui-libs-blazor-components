using ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class Reset
    {
        [Fact]
        public void Should_invoke_reset_for_each_instance()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var resetValueIterations = new List<Foo>();
            var exceptions = new List<Exception>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                ResetValue = resetValueIterations.Add
            };

            // Act
            propertyDescriptor.ResetValue<Foo, string>([instance1, instance2], exceptions);

            // Assert
            resetValueIterations.Should().BeEquivalentTo([instance1, instance2]);
        }

        [Fact]
        public void Should_communicate_missing_reset_value_assignment_with_exception()
        {
            // Arrange
            var exceptions = new List<Exception>();

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name
            };

            // Act
            propertyDescriptor.ResetValue<Foo, string>([], exceptions);

            // Assert
            exceptions.Cast<ResetValueException>().Should().HaveCount(1);
        }

        [Fact]
        public void Should_propagate_reset_value_exceptions()
        {
            // Arrange
            var instance = new Foo();
            var exceptions = new List<Exception>();
            var resetValueException = new ResetValueException("Something went wrong.");

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ResetValue = _ => throw resetValueException
            };

            // Act
            propertyDescriptor.ResetValue<Foo, string>([instance], exceptions);

            // Assert
            exceptions.Should().HaveCount(1).And.Contain(resetValueException);
        }

        [Fact]
        public void Should_propagate_unexpected_exceptions()
        {
            // Arrange
            var instance = new Foo();
            var exceptions = new List<Exception>();
            var unexpectedException = new InvalidOperationException("Oh snap!");

            var propertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                ResetValue = _ => throw unexpectedException
            };

            // Act
            propertyDescriptor.ResetValue<Foo, string>([instance], exceptions);

            // Assert
            exceptions.Should().HaveCount(1);
            exceptions[0].InnerException.Should().BeSameAs(unexpectedException);
        }
    }
}
