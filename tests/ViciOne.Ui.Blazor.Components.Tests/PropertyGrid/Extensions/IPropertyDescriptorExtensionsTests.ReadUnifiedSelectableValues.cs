using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    public sealed class ReadUnifiedSelectableValues
    {
        private readonly IEqualityComparer<string> _valueEqualityComparer = EqualityComparer<string>.Default;

        private readonly record struct SelectableValue(string Value, string Text) : ISelectableValue<string>;

        [Fact]
        public void Should_return_all_values_when_instances_have_identical_selectable_values()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var selectableValues = new List<ISelectableValue<string>>
            {
                new SelectableValue("A", "Lorem"),
                new SelectableValue("B", "Ipsum")
            };

            var propertyDescriptor = new SelectionPropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                GetSelectableValues = _ => selectableValues
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedSelectableValues([instance1, instance2], _valueEqualityComparer);

            // Assert
            result.Should().NotBeNull().And.BeEquivalentTo(selectableValues);
        }

        [Fact]
        public void Shoudl_return_intersection_of_values_when_some_values_are_shared()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var selectableValues1 = new List<ISelectableValue<string>>
            {
                new SelectableValue("A", "Lorem"),
                new SelectableValue("B", "Ipsum"),
                new SelectableValue("C", "Dolor")
            };

            var selectableValues2 = new List<ISelectableValue<string>>
            {
                new SelectableValue("B", "Sit"),
                new SelectableValue("C", "Amet"),
                new SelectableValue("D", "Consetetur"),
                new SelectableValue("E", "Sadipscing")
            };

            var propertyDescriptor = new SelectionPropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                GetSelectableValues = instance =>
                {
                    if (instance == instance1)
                        return selectableValues1;

                    return selectableValues2;
                }
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedSelectableValues([instance1, instance2], _valueEqualityComparer);

            // Assert
            result.Should().NotBeNull()
                .And.Subject.Select(v => v.Value).Should().BeEquivalentTo("B", "C");
        }

        [Fact]
        public void Should_return_null_when_no_common_values_and_differences_were_found()
        {
            // Arrange
            var instance1 = new Foo();
            var instance2 = new Foo();

            var firstValues = new List<ISelectableValue<string>>
            {
                new SelectableValue("A", "Lorem"),
                new SelectableValue("B", "Ipsum")
            };

            var secondValues = new List<ISelectableValue<string>>
            {
                new SelectableValue("C", "Sit"),
                new SelectableValue("D", "Amet")
            };

            var propertyDescriptor = new SelectionPropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                GetSelectableValues = instance =>
                {
                    if (instance == instance1)
                        return firstValues;

                    return secondValues;
                }
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedSelectableValues([instance1, instance2], _valueEqualityComparer);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Returns_empty_list_when_no_instances_are_passed()
        {
            // Arrange
            var propertyDescriptor = new SelectionPropertyDescriptor<Foo, string>
            {
                Name = nameof(Foo.Name),
                GetValue = instance => instance.Name,
                GetSelectableValues = _ => []
            };

            // Act
            var result = propertyDescriptor.ReadUnifiedSelectableValues([], _valueEqualityComparer);

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }
    }
}
