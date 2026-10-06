using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.TestingHelpers.TextBox.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyGridTests
{
    public sealed class ReverseAlphabeticalPropertyComparer
    {
        [Fact]
        public void Should_sort_properties_z_to_a_in_flat_mode()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Alpha"),
                CreatePropertyGridItem("Zebra"),
                CreatePropertyGridItem("Mango"),
            ];

            // Act
            var component = testContext.Render<PropertyEntrySet>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items)
                .Add(p => p.PropertyComparer, testContext.Services.GetRequiredService<IReverseAlphabeticalPropertyComparer>()));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Zebra", "Mango", "Alpha");
        }

        [Fact]
        public void Should_sort_properties_z_to_a_within_group()
        {
            // Arrange
            using var testContext = CreatePropertyEntrySetTestContext();
            var controller = Substitute.For<IPropertyGridController>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Alpha", "Cat"),
                CreatePropertyGridItem("Zebra", "Cat"),
                CreatePropertyGridItem("Mango", "Cat"),
            ];

            // Act
            var component = testContext.Render<PropertyGroup>(b => b
                .AddCascadingValue(controller)
                .Add(p => p.Items, items)
                .Add(p => p.PropertyComparer, testContext.Services.GetRequiredService<IReverseAlphabeticalPropertyComparer>()));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Zebra", "Mango", "Alpha");
        }

        [Fact]
        public void Should_sort_properties_z_to_a_in_grouped_mode()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.SetupForTextBox();
            testContext.Services.AddPropertyGrid<object>();

            var state = testContext.Services.GetRequiredService<IPropertyGridState<object>>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("Alpha", "A"),
                CreatePropertyGridItem("Zebra", "A"),
                CreatePropertyGridItem("Beta", "B"),
                CreatePropertyGridItem("Mango", "B"),
            ];
            state.GroupByCategory = true;
            state.Items = items;
            state.PropertyComparer = testContext.Services.GetRequiredService<IReverseAlphabeticalPropertyComparer>();

            var controller = testContext.Services.GetRequiredService<IPropertyGridController<object>>();

            // Act
            var component = testContext.Render<PropertyGrid<object>>(b => b
                .Add(p => p.Controller, controller));

            // Assert
            var names = component.FindAll(".property-name").Select(e => e.TextContent.Trim()).ToList();
            names.Should().Equal("Zebra", "Alpha", "Mango", "Beta");
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
