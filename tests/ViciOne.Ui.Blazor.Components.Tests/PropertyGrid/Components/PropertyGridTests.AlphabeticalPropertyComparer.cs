using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyGridTests
{
    public sealed class AlphabeticalPropertyComparer
    {
        [Fact]
        public void Should_sort_properties_a_to_z_by_default_in_flat_mode()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Zebra"),
                CreatePropertyGridItem("Alpha"),
                CreatePropertyGridItem("Mango"),
            ];

            // Act
            var component = testContext.Render<PropertyEntrySet>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Alpha", "Mango", "Zebra");
        }

        [Fact]
        public void Should_sort_properties_a_to_z_with_explicit_comparer_in_flat_mode()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Zebra"),
                CreatePropertyGridItem("Alpha"),
                CreatePropertyGridItem("Mango"),
            ];

            // Act
            var component = testContext.Render<PropertyEntrySet>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items)
                .Add(p => p.PropertyComparer, testContext.Services.GetRequiredService<IAlphabeticalPropertyComparer>()));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Alpha", "Mango", "Zebra");
        }

        [Fact]
        public void Should_sort_properties_a_to_z_by_default_within_group()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Zebra", "Cat"),
                CreatePropertyGridItem("Alpha", "Cat"),
                CreatePropertyGridItem("Mango", "Cat"),
            ];

            // Act
            var component = testContext.Render<PropertyGroup>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Alpha", "Mango", "Zebra");
        }

        [Fact]
        public void Should_sort_properties_a_to_z_with_explicit_comparer_within_group()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Zebra", "Cat"),
                CreatePropertyGridItem("Alpha", "Cat"),
                CreatePropertyGridItem("Mango", "Cat"),
            ];

            // Act
            var component = testContext.Render<PropertyGroup>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items)
                .Add(p => p.PropertyComparer, testContext.Services.GetRequiredService<IAlphabeticalPropertyComparer>()));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Alpha", "Mango", "Zebra");
        }
        private static BunitContext CreatePropertyEntrySetTestContext()
        {
            var context = new BunitContext();
            context.Services.AddTooltip();
            context.Services.AddCategoryComparers().AddPropertyComparers();
            context.Services.AddScoped(_ => Substitute.For<IPropertyEditorComponentRegistry>());
            return context;
        }
    }
}
