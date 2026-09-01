using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyOperationIterationExtensionsTests
{
    public sealed class ReadUnifiedCanSetValue
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void Assert_result_based_on_given_enabled_state_per_instance(bool fooEnabled,
            bool barEnabled, bool expectedResult)
        {
            // Arrange
            var foo1 = new Foo();
            var foo2 = new Foo();

            var fooPropertyDescriptor = new PropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                Enabled = _ => fooEnabled
            };

            var bar = new Bar();

            var barPropertyDescriptor = new PropertyDescriptor<Bar, string>
            {
                Name = nameof(Bar.Name),
                GetValue = instance => instance.Name,
                SetValue = (instance, value) => instance.Name = value,
                Enabled = _ => barEnabled
            };

            var fooPropertyOperationIteration = new PropertyOperationIteration
            {
                InstanceType = typeof(Foo),
                PropertyValueType = typeof(string),
                PropertyDescriptor = fooPropertyDescriptor,
#pragma warning disable IDE0300 // Simplify collection initialization
                Instances = new[] { foo1, foo2 }
#pragma warning restore IDE0300 // Simplify collection initialization
            };

            var barPropertyOperationIteration = new PropertyOperationIteration
            {
                InstanceType = typeof(Bar),
                PropertyValueType = typeof(string),
                PropertyDescriptor = barPropertyDescriptor,
#pragma warning disable IDE0300 // Simplify collection initialization
                Instances = new[] { bar }
#pragma warning restore IDE0300 // Simplify collection initialization
            };

            IReadOnlyList<PropertyOperationIteration> propertyOperationIterations =
                [fooPropertyOperationIteration, barPropertyOperationIteration];

            // Act
            var result = propertyOperationIterations.ReadUnifiedCanSetValue();

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
