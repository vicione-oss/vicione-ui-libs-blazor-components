using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyGridTests
{
    public sealed class AlphabeticalCategoryComparer
    {
        [Fact]
        public void Should_sort_categories_a_to_z_by_default_in_grouped_mode()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.Mode = JSRuntimeMode.Loose;
            testContext.Services.AddPropertyGrid<object>();

            var state = testContext.Services.GetRequiredService<IPropertyGridState<object>>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("P1", "Gamma"),
                CreatePropertyGridItem("P2", "Alpha"),
                CreatePropertyGridItem("P3", "Beta"),
            ];
            state.GroupByCategory = true;
            state.Items = items;

            var controller = testContext.Services.GetRequiredService<IPropertyGridController<object>>();

            // Act
            var component = testContext.Render<PropertyGrid<object>>(b => b
                .Add(p => p.Controller, controller));

            // Assert
            var categories = component.FindAll(".group-name").Select(e => e.TextContent.Trim()).ToList();
            categories.Should().Equal("Alpha", "Beta", "Gamma");
        }

        [Fact]
        public void Should_sort_categories_a_to_z_with_explicit_comparer_in_grouped_mode()
        {
            // Arrange
            using var testContext = new BunitContext();
            testContext.JSInterop.Mode = JSRuntimeMode.Loose;
            testContext.Services.AddPropertyGrid<object>();

            var state = testContext.Services.GetRequiredService<IPropertyGridState<object>>();
            IPropertyGridItem[] items =
            [
                CreatePropertyGridItem("P1", "Gamma"),
                CreatePropertyGridItem("P2", "Alpha"),
                CreatePropertyGridItem("P3", "Beta"),
            ];
            state.GroupByCategory = true;
            state.Items = items;
            state.CategoryComparer = testContext.Services.GetRequiredService<IAlphabeticalCategoryComparer>();

            var controller = testContext.Services.GetRequiredService<IPropertyGridController<object>>();

            // Act
            var component = testContext.Render<PropertyGrid<object>>(b => b
                .Add(p => p.Controller, controller));

            // Assert
            var categories = component.FindAll(".group-name").Select(e => e.TextContent.Trim()).ToList();
            categories.Should().Equal("Alpha", "Beta", "Gamma");
        }
    }
}
